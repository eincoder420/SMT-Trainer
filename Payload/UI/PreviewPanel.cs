using System;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Thumbnail preview of the specific asset being edited - the hair mesh, a garment, the
    // head - rendered live with its current material so colour changes show immediately.
    //
    // The camera frames whatever Transform the page supplies, using that object's renderer
    // bounds, so each page gets a tight shot of its own subject rather than a view of the
    // whole character. The far clip plane is pulled in just behind the subject, which keeps
    // the room behind it out of the shot.
    public class PreviewPanel
    {
        private const int RT_WIDTH  = 420;
        private const int RT_HEIGHT = 560;
        private const int UI_LAYER  = 5;      // built-in UI layer, excluded so the HUD isn't captured
        private const float FOV     = 32f;

        private Camera _camera;
        private RenderTexture _target;

        // Yaw is an offset from the character's own forward, so 0 always looks at her face
        // no matter which way she happens to be standing in the world.
        private float _yaw;
        private float _zoom = 1f;
        private bool _hasSubject;

        public bool AutoRotate;

        public bool Ready => _camera != null && _target != null;

        // ─── Lifecycle ────────────────────────────────────────────────────────────
        private void Ensure(Transform parent)
        {
            if (_target == null)
            {
                _target = new RenderTexture(RT_WIDTH, RT_HEIGHT, 24)
                {
                    name = "SMT_PreviewRT",
                    hideFlags = HideFlags.HideAndDontSave,
                    antiAliasing = 2
                };
                _target.Create();
            }

            if (_camera != null) return;

            var go = new GameObject("SMT_PreviewCamera") { hideFlags = HideFlags.HideAndDontSave };
            go.transform.SetParent(parent, false);

            _camera = go.AddComponent<Camera>();
            _camera.targetTexture = _target;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.06f, 0.05f, 0.09f, 1f);
            _camera.cullingMask = ~(1 << UI_LAYER);
            _camera.fieldOfView = FOV;
            _camera.nearClipPlane = 0.01f;
            // Rendered on demand from Render(), so it costs nothing while the menu is closed.
            _camera.enabled = false;
        }

        public void Dispose()
        {
            if (_camera != null) { UnityEngine.Object.Destroy(_camera.gameObject); _camera = null; }
            if (_target != null) { _target.Release(); UnityEngine.Object.Destroy(_target); _target = null; }
        }

        // ─── Bounds ───────────────────────────────────────────────────────────────
        // Combined world bounds of every enabled renderer under root.
        // Skinned meshes report their posed bounds, which is what we want for worn items.
        private static bool TryGetBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            if (root == null) return false;

            bool found = false;
            foreach (var r in root.GetComponentsInChildren<Renderer>(false))
            {
                if (r == null || !r.enabled) continue;
                if (!found) { bounds = r.bounds; found = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return found && bounds.extents.sqrMagnitude > 0.0000001f;
        }

        // The direction the character faces. Taken from the player root where possible: a
        // hair or garment mesh has its own arbitrary orientation, so using the subject's
        // own forward would point the camera somewhere random.
        private static Vector3 CharacterForward(Transform subject)
        {
            var root = Features.GameRefs.Player != null
                     ? Features.GameRefs.Player.transform
                     : Features.PreviewTargets.Body();

            Vector3 forward = root != null ? root.forward : (subject != null ? subject.forward : Vector3.forward);
            forward.y = 0f;
            return forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;
        }

        // ─── Input ────────────────────────────────────────────────────────────────
        public void HandleInput()
        {
            // Deliberately not arrows or numpad - those drive the menu itself.
            if (Input.GetKey(KeyCode.Home))     _yaw -= 90f * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.End))      _yaw += 90f * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.PageUp))   _zoom = Mathf.Clamp(_zoom - 0.8f * Time.unscaledDeltaTime, 0.35f, 3f);
            if (Input.GetKey(KeyCode.PageDown)) _zoom = Mathf.Clamp(_zoom + 0.8f * Time.unscaledDeltaTime, 0.35f, 3f);
            if (Input.GetKeyDown(KeyCode.Delete)) AutoRotate = !AutoRotate;

            if (AutoRotate) _yaw += 30f * Time.unscaledDeltaTime;
        }

        // Renders one frame of target. Called from LateUpdate, not OnGUI:
        // manually rendering a camera during a GUI event is not safe, and LateUpdate runs
        // after the character has been posed for this frame.
        public void Render(Transform parent, Func<Transform> target, float zoomBias = 1f)
        {
            _hasSubject = false;
            if (target == null) return;

            Transform subject;
            try { subject = target(); }
            catch { return; }

            if (!TryGetBounds(subject, out var bounds)) return;

            Ensure(parent);
            if (!Ready) return;

            // Distance that fits the subject's bounding sphere in the vertical FOV.
            float radius = bounds.extents.magnitude;
            float distance = radius / Mathf.Tan(FOV * 0.5f * Mathf.Deg2Rad) * 1.05f * _zoom * zoomBias;

            // Start from the character's forward so the default view is her face, then apply
            // the user's yaw on top and lift the camera slightly for a natural angle.
            Vector3 forward = CharacterForward(subject);
            Vector3 dir = Quaternion.AngleAxis(_yaw, Vector3.up) * forward;

            _camera.transform.position = bounds.center + dir * distance + Vector3.up * (radius * 0.12f);
            _camera.transform.LookAt(bounds.center);

            // Clip just past the subject so the surrounding room does not show up behind it.
            _camera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 2f);
            _camera.farClipPlane  = distance + radius * 2.5f;

            _camera.Render();
            _hasSubject = true;
        }

        // ─── Drawing ──────────────────────────────────────────────────────────────
        public void Draw(Rect rect, MenuTheme theme, string caption, string emptyMessage = null)
        {
            const float headerH = 26f;
            const float footerH = 22f;

            var header = new Rect(rect.x, rect.y, rect.width, headerH);
            MenuStyle.Fill(header, theme.HdrMid);
            MenuStyle.Fill(new Rect(rect.x, rect.y + headerH - 2f, rect.width, 2f), theme.Accent);
            MenuStyle.Text(new Rect(rect.x + 10f, rect.y, rect.width - 20f, headerH),
                           string.IsNullOrEmpty(caption) ? "PREVIEW" : caption.ToUpperInvariant(),
                           12, Color.white, TextAnchor.MiddleLeft, bold: true);

            var view = new Rect(rect.x, rect.y + headerH, rect.width, rect.height - headerH - footerH);
            MenuStyle.Fill(view, new Color32(14, 12, 22, 240));

            if (Ready && _hasSubject)
                GUI.DrawTexture(view, _target, ScaleMode.ScaleToFit, false);
            else
                MenuStyle.Text(view,
                               string.IsNullOrEmpty(emptyMessage) ? "nothing to preview" : emptyMessage,
                               12, new Color(1f, 1f, 1f, 0.45f), TextAnchor.MiddleCenter);

            var footer = new Rect(rect.x, view.yMax, rect.width, footerH);
            MenuStyle.Fill(footer, new Color32(0, 0, 0, 235));
            MenuStyle.Text(new Rect(footer.x + 8f, footer.y, footer.width - 16f, footerH),
                           AutoRotate ? "Home/End turn • PgUp/PgDn zoom • Del spin:ON"
                                      : "Home/End turn • PgUp/PgDn zoom • Del spin",
                           10, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft);
        }
    }
}
