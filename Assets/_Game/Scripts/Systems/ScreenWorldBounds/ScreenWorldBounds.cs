using System;
using UnityEngine;

public sealed class ScreenWorldBounds : IScreenWorldBounds
{
    private const float MATRIX_TOLERANCE = 0.000001f;

    private Camera _camera;

    private float _worldZ;

    private ScreenWorldBoundsArea _bounds;

    private Matrix4x4 _view;

    private Matrix4x4 _projection;

    private Rect _outputRect;

    private RenderTexture _target;

    private bool _hasCache;

    private Vector4 _lens;

    private Matrix4x4 _pose;

    public bool IsValid
    {
        get
        {
            return TryGetBounds(out _);
        }
    }

    public string LastError { get; private set; }

    public ScreenWorldBounds(Camera camera, float worldZ = 0.0f)
    {
        Init(camera, worldZ);
    }

    public bool Init(Camera camera, float worldZ = 0.0f)
    {
        _camera = camera;
        _worldZ = worldZ;
        _hasCache = false;

        return TryGetBounds(out _);
    }

    public bool TryGetBounds(out ScreenWorldBoundsArea bounds)
    {
        bounds = default;

        if (_camera == null)
        {
            return Fail("카메라가 없거나 파괴,비활성화됐습니다. 활성 게임 Camera를 Init으로 전달하세요.");
        }

        if (!_camera.isActiveAndEnabled)
        {
            return Fail("카메라가 없거나 파괴,비활성화됐습니다. 활성 게임 Camera를 Init으로 전달하세요.");
        }

        if (!_camera.orthographic)
        {
            return Fail("기본 디스플레이의 비스테레오 직교 Camera만 지원합니다. 카메라 설정을 확인하세요.");
        }

        if (_camera.stereoEnabled)
        {
            return Fail("기본 디스플레이의 비스테레오 직교 Camera만 지원합니다. 카메라 설정을 확인하세요.");
        }

        if (_camera.targetDisplay != 0)
        {
            return Fail("기본 디스플레이의 비스테레오 직교 Camera만 지원합니다. 카메라 설정을 확인하세요.");
        }

        if (IsCacheCurrent())
        {
            bounds = _bounds;
            return true;
        }

        if (!TryCalculate(_camera, _worldZ, out ScreenWorldBoundsArea next, out string error))
        {
            return Fail(error);
        }

        CacheBounds(next);
        bounds = next;
        return true;
    }

    private bool IsCacheCurrent()
    {
        if (!_hasCache)
        {
            return false;
        }

        if (!_camera.worldToCameraMatrix.Equals(_view))
        {
            return false;
        }

        if (!_camera.projectionMatrix.Equals(_projection))
        {
            return false;
        }

        if (!GetOutputRect(_camera).Equals(_outputRect))
        {
            return false;
        }

        if (_camera.targetTexture != _target)
        {
            return false;
        }

        if (!GetLens(_camera).Equals(_lens))
        {
            return false;
        }

        return _camera.transform.localToWorldMatrix.Equals(_pose);
    }

    private void CacheBounds(ScreenWorldBoundsArea bounds)
    {
        _bounds = bounds;
        _view = _camera.worldToCameraMatrix;
        _projection = _camera.projectionMatrix;
        _outputRect = GetOutputRect(_camera);
        _target = _camera.targetTexture;
        _lens = GetLens(_camera);
        _pose = _camera.transform.localToWorldMatrix;

        _hasCache = true;
        LastError = null;
    }

    internal static bool TryCalculate(
        Camera camera,
        float worldZ,
        out ScreenWorldBoundsArea bounds,
        out string error)
    {
        bounds = default;
        error = "활성 기본 직교 Camera, 유한한 크기,종횡비,clip 범위,worldZ,표시 영역을 확인하세요.";

        if (camera == null)
        {
            return false;
        }

        if (!camera.isActiveAndEnabled)
        {
            return false;
        }

        if (!camera.orthographic)
        {
            return false;
        }

        if (camera.stereoEnabled)
        {
            return false;
        }

        if (camera.targetDisplay != 0)
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(worldZ))
        {
            return false;
        }

        Vector3 position = camera.transform.position;
        Vector4 lens = GetLens(camera);
        float depth = worldZ - position.z;

        if (!HasValidViewport(camera))
        {
            error = "표시 영역이 화면 안의 양수 직사각형이어야 합니다. Camera.rect를 0~1 범위로 설정하세요.";
            return false;
        }

        if (!HasValidCameraParameters(camera, position, lens, depth))
        {
            return false;
        }

        if (!HasSupportedMatrices(camera, position, lens))
        {
            error = "회전,기울기,사용자 뷰/투영 행렬은 지원하지 않습니다. Camera 회전을 0으로 하고 기본 행렬을 사용하세요.";
            return false;
        }

        return TryCreateBounds(camera, depth, worldZ, out bounds, out error);
    }

    private static Rect GetOutputRect(Camera camera)
    {
        RenderTexture target = camera.targetTexture;

        if (target == null)
        {
            return camera.pixelRect;
        }

        return new Rect(0, 0, target.width, target.height);
    }

    private static Vector4 GetLens(Camera camera)
    {
        return new Vector4(
            camera.orthographicSize,
            camera.aspect,
            camera.nearClipPlane,
            camera.farClipPlane);
    }

    private static bool HasValidViewport(Camera camera)
    {
        if (camera.targetTexture != null)
        {
            return true;
        }

        Rect viewport = camera.rect;

        if (!IsValidOutputRect(viewport))
        {
            return false;
        }

        if (viewport.x < 0.0f)
        {
            return false;
        }

        if (viewport.y < 0.0f)
        {
            return false;
        }

        if (viewport.xMax > 1.0f)
        {
            return false;
        }

        return viewport.yMax <= 1.0f;
    }

    private static bool HasValidCameraParameters(
        Camera camera,
        Vector3 position,
        Vector4 lens,
        float depth)
    {
        if (!ScreenWorldBoundsArea.IsFinite(position))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(depth))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(lens.x))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(lens.y))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(lens.z))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(lens.w))
        {
            return false;
        }

        if (lens.x <= 0.0f)
        {
            return false;
        }

        if (lens.y <= 0.0f)
        {
            return false;
        }

        if (lens.w <= lens.z)
        {
            return false;
        }

        if (depth < lens.z)
        {
            return false;
        }

        if (depth > lens.w)
        {
            return false;
        }

        return IsValidOutputRect(GetOutputRect(camera));
    }

    private static bool IsValidOutputRect(Rect rect)
    {
        if (!ScreenWorldBoundsArea.IsFinite(rect.x))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(rect.y))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(rect.width))
        {
            return false;
        }

        if (!ScreenWorldBoundsArea.IsFinite(rect.height))
        {
            return false;
        }

        if (rect.width <= 0.0f)
        {
            return false;
        }

        return rect.height > 0.0f;
    }

    private static bool HasSupportedMatrices(Camera camera, Vector3 position, Vector4 lens)
    {
        if (!Matches(Matrix4x4.Rotate(camera.transform.rotation), Matrix4x4.identity))
        {
            return false;
        }

        Matrix4x4 expectedView = Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.Translate(-position);

        if (!Matches(camera.worldToCameraMatrix, expectedView))
        {
            return false;
        }

        float halfWidth = lens.x * lens.y;
        Matrix4x4 expectedProjection = Matrix4x4.Ortho(
            -halfWidth, halfWidth, -lens.x, lens.x, lens.z, lens.w);

        return Matches(camera.projectionMatrix, expectedProjection);
    }

    private static bool TryCreateBounds(
        Camera camera,
        float depth,
        float worldZ,
        out ScreenWorldBoundsArea bounds,
        out string error)
    {
        bounds = default;

        Vector3 min = camera.ViewportToWorldPoint(new Vector3(0, 0, depth));
        Vector3 max = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
        min.z = worldZ;
        max.z = worldZ;

        try
        {
            bounds = new ScreenWorldBoundsArea(min, max);
        }
        catch (ArgumentException)
        {
            error = "계산 결과가 비유한 값 또는 0 이하 크기입니다. Camera 위치,크기,종횡비를 확인하세요.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool Matches(Matrix4x4 actual, Matrix4x4 expected)
    {
        for (int i = 0; i < 16; i++)
        {
            if (!ScreenWorldBoundsArea.IsFinite(actual[i]))
            {
                return false;
            }

            if (!ScreenWorldBoundsArea.IsFinite(expected[i]))
            {
                return false;
            }

            if (Mathf.Abs(actual[i] - expected[i]) > MATRIX_TOLERANCE)
            {
                return false;
            }
        }

        return true;
    }

    private bool Fail(string error)
    {
        _hasCache = false;
        LastError = error;
        return false;
    }
}
