using System.Collections.Generic;
using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Time of day, time freeze, and weather - all driven through the game's Azure[Sky] asset.
    public static class WorldFeatures
    {
        private const float DEFAULT_DAY_LENGTH = 24f;

        private static bool  _frozen;
        private static float _savedDayLength = DEFAULT_DAY_LENGTH;

        // ─── Time of day ──────────────────────────────────────────────────────────
        // Hours, 0-24.
        public static float Timeline
        {
            get => GameRefs.Time_ != null ? GameRefs.Time_.timeline : 0f;
            set
            {
                if (GameRefs.Time_ == null) return;
                GameRefs.Time_.timeline = Mathf.Clamp(value, 0f, 24f);
            }
        }

        public static string ClockText
        {
            get
            {
                if (GameRefs.Time_ == null) return "--:--";
                float t = GameRefs.Time_.timeline;
                int h = Mathf.Clamp(Mathf.FloorToInt(t), 0, 23);
                int m = Mathf.Clamp(Mathf.FloorToInt(t * 60f % 60f), 0, 59);
                return h.ToString("00") + ":" + m.ToString("00");
            }
        }

        // Freeze works by zeroing dayLength: AzureTimeController derives its per-frame step
        // as 0.4/dayLength and treats dayLength &lt;= 0 as "no progression", so the clock
        // stops without touching Time.timeScale or fighting the game every frame.
        public static bool FreezeTime
        {
            get => _frozen;
            set
            {
                if (GameRefs.Time_ == null) return;
                if (value == _frozen) return;

                if (value)
                {
                    _savedDayLength = GameRefs.Time_.dayLength > 0f
                                    ? GameRefs.Time_.dayLength : DEFAULT_DAY_LENGTH;
                    GameRefs.Time_.dayLength = 0f;
                }
                else
                {
                    GameRefs.Time_.dayLength = _savedDayLength > 0f
                                             ? _savedDayLength : DEFAULT_DAY_LENGTH;
                }
                _frozen = value;
            }
        }

        // Length of a full day in hours; lower means faster time.
        public static float DayLength
        {
            get => _frozen ? _savedDayLength
                 : (GameRefs.Time_ != null ? GameRefs.Time_.dayLength : DEFAULT_DAY_LENGTH);
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 240f);
                _savedDayLength = v;
                if (!_frozen && GameRefs.Time_ != null) GameRefs.Time_.dayLength = v;
            }
        }

        // ─── Weather ──────────────────────────────────────────────────────────────
        public static bool HasWeather
        {
            get
            {
                var sky = GameRefs.Sky;
                return sky != null && sky.globalWeatherList != null && sky.globalWeatherList.Count > 0;
            }
        }

        // Names of the game's own weather profiles, in the order Azure indexes them.
        public static IList<string> WeatherNames()
        {
            var names = new List<string>();
            var sky = GameRefs.Sky;
            if (sky?.globalWeatherList == null) { names.Add("n/a"); return names; }

            for (int i = 0; i < sky.globalWeatherList.Count; i++)
            {
                var profile = sky.globalWeatherList[i].profile;
                names.Add(profile != null ? profile.name : "Profile " + i);
            }
            if (names.Count == 0) names.Add("n/a");
            return names;
        }

        public static int WeatherIndex
        {
            get
            {
                var sky = GameRefs.Sky;
                if (sky == null) return 0;
                return Mathf.Max(0, sky.globalWeatherIndex);
            }
            set
            {
                var sky = GameRefs.Sky;
                if (sky?.globalWeatherList == null || sky.globalWeatherList.Count == 0) return;
                int i = Mathf.Clamp(value, 0, sky.globalWeatherList.Count - 1);
                // The game's own transition routine - keeps the blend and events intact.
                EditorFeatures.Safe(() => sky.SetNewWeatherProfile(i));
            }
        }

        // Skip the blend and snap straight to the selected weather.
        public static void ApplyWeatherInstantly()
        {
            var sky = GameRefs.Sky;
            if (sky == null) return;
            EditorFeatures.Safe(() =>
            {
                sky.globalWeatherTransitionTime = 0f;
                sky.SetNewWeatherProfile(WeatherIndex);
            });
        }

        // Drop back to the profile the game would use on its own.
        public static void ResetWeather()
        {
            var sky = GameRefs.Sky;
            if (sky == null) return;
            EditorFeatures.Safe(() => sky.SetNewWeatherProfile(-1));
        }
    }
}
