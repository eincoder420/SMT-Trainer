using SamanthaTrainer.Payload.Features;
using SamanthaTrainer.Payload.UI;
using UnityEngine;

namespace SamanthaTrainer.Payload
{
    // Host component. Owns the menu, drives the per-frame feature loop, and keeps the
    // cached game references fresh across scene loads.
    public class TrainerBehaviour : MonoBehaviour
    {
        public static TrainerBehaviour Instance { get; private set; }

        private NativeMenu _menu;
        private TextPrompt _prompt;

        private void Awake()
        {
            Instance = this;
            _prompt = new TextPrompt();
            _menu = new NativeMenu(MenuBuilder.Build(_prompt));
            GameRefs.Rescan();
        }

        private void Update()
        {
            GameRefs.Tick();
            PlayerFeatures.Tick();
            StuckFix.Tick();

            // Insert always toggles, but the prompt takes priority so typing "Insert" as
            // part of a value can't close the menu underneath it.
            if (!_prompt.Active && Input.GetKeyDown(KeyCode.Insert))
                _menu.Visible = !_menu.Visible;

            // Direct hotkey: being stuck is exactly when you don't want to go menu-diving.
            if (!_prompt.Active && Input.GetKeyDown(KeyCode.F9))
                UI.Toast.Show(StuckFix.Apply());

            if (!_prompt.Active)
                _menu.HandleInput();
        }

        // The preview camera is rendered here rather than in OnGUI - manually rendering a
        // camera during a GUI event is not safe, and LateUpdate runs after the character
        // has been posed for this frame.
        private void LateUpdate()
        {
            _menu.Preview.Render(transform, _menu.CurrentPreviewTarget, _menu.CurrentPreviewZoom);
        }

        private void OnGUI()
        {
            _menu.Draw();
            _prompt.Draw();
            Toast.Draw();
        }

        private void OnDestroy()
        {
            PlayerFeatures.Reset();
            _menu?.Preview?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
