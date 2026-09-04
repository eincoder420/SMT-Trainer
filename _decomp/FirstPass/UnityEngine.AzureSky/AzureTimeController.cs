using System;
using UnityEngine.UI;

namespace UnityEngine.AzureSky;

[ExecuteInEditMode]
[AddComponentMenu("Azure[Sky]/Azure Time Controller")]
[RequireComponent(typeof(AzureSkyController))]
public class AzureTimeController : MonoBehaviour
{
	private AzureSkyController m_skyController;

	public AzureTimeSystem timeSystem;

	public AzureTimeDirection timeDirection;

	public AzureTimeRepeatMode repeatMode;

	public float timeline = 6f;

	private float m_timeOfDay = 6f;

	private float m_sunElevation;

	private float m_moonElevation;

	private float m_timeProgressionStep;

	private bool m_isTimelineTransitionInProgress;

	private float m_timelineSourceTransitionTime;

	private float m_timelineDestinationTransitionTime;

	private float m_startTimelineTransitionStep;

	private float m_timelineTransitionStep;

	private float m_timelineTransitionSpeed;

	public int hour = 6;

	public int minute;

	public int day = 1;

	public int month = 1;

	public int year = 2020;

	public int selectedCalendarDay = 1;

	public float latitude;

	public float longitude;

	public float utc;

	public float dayLength = 24f;

	public bool isTimeEvaluatedByCurve;

	public AnimationCurve dayLengthCurve = AnimationCurve.Linear(0f, 0f, 24f, 24f);

	private DateTime m_dateTime;

	private int m_previousHour = 6;

	private int m_previousMinute;

	private int m_daysInMonth = 30;

	private int m_previousDaysInMonth = 30;

	private int m_previousMonth = 1;

	private int m_dayOfWeek;

	private Vector3 m_starFieldOffset = Vector3.zero;

	private Matrix4x4 m_starFieldMatrix;

	private Vector3 m_sunLocalDirection;

	private Vector3 m_moonLocalDirection;

	private Vector3 m_sunRealisticRotation;

	private Vector3 m_moonRealisticRotation;

	private Quaternion m_sunSimpleRotation;

	private Quaternion m_moonSimpleRotation;

	private float m_lst;

	private float m_radians;

	private float m_radLatitude;

	private float m_sinLatitude;

	private float m_cosLatitude;

	public readonly string[] WeekList = new string[7] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

	public readonly string[] MonthList = new string[12]
	{
		"January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
		"November", "December"
	};

	public readonly string[] DayList = new string[42]
	{
		"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		"10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
		"20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
		"30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
		"40", "41"
	};

	private void Reset()
	{
		UpdateCalendar();
	}

	private void Start()
	{
		m_skyController = GetComponent<AzureSkyController>();
		m_timeProgressionStep = GetTimeProgressionStep();
		m_previousMinute = minute;
		m_previousHour = hour;
		UpdateTimeSystem();
		UpdateCalendar();
	}

	private void Update()
	{
		m_timeOfDay = (isTimeEvaluatedByCurve ? dayLengthCurve.Evaluate(timeline) : timeline);
		hour = (int)Mathf.Floor(m_timeOfDay);
		minute = (int)Mathf.Floor(m_timeOfDay * 60f % 60f);
		if (!Application.isPlaying)
		{
			return;
		}
		if (timeDirection == AzureTimeDirection.Forward)
		{
			timeline += m_timeProgressionStep * Time.deltaTime;
			if (m_isTimelineTransitionInProgress)
			{
				DoTimelineTransition(m_timelineSourceTransitionTime, m_timelineDestinationTransitionTime);
			}
			if (timeline > 24f)
			{
				IncreaseDay();
				m_skyController.OnDayChange();
				timeline = 0f;
			}
		}
		else
		{
			timeline -= m_timeProgressionStep * Time.deltaTime;
			if (m_isTimelineTransitionInProgress)
			{
				DoTimelineTransition(m_timelineSourceTransitionTime, m_timelineDestinationTransitionTime);
			}
			if (timeline < 0f)
			{
				DecreaseDay();
				m_skyController.OnDayChange();
				timeline = 24f;
			}
		}
		if (m_previousMinute != minute)
		{
			m_skyController.onMinuteChange?.Invoke();
			m_previousMinute = minute;
		}
		if (m_previousHour != hour)
		{
			m_skyController.onHourChange?.Invoke();
			m_previousHour = hour;
		}
		UpdateTimeSystem();
	}

	public void UpdateTimeSystem()
	{
		switch (timeSystem)
		{
		case AzureTimeSystem.Simple:
			m_sunSimpleRotation = GetSunSimpleRotation();
			m_skyController.sunTransform.localRotation = m_sunSimpleRotation;
			m_moonSimpleRotation = m_sunSimpleRotation * Quaternion.Euler(0f, -180f, 0f);
			m_skyController.moonTransform.localRotation = m_moonSimpleRotation;
			m_starFieldMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(m_skyController.starFieldPosition), Vector3.one).inverse * m_skyController.sunTransform.transform.worldToLocalMatrix;
			if (m_skyController.shaderUpdateMode == AzureShaderUpdateMode.ByMaterial)
			{
				m_skyController.skyMaterial.SetMatrix(AzureShaderUniforms.StarFieldMatrix, m_starFieldMatrix);
			}
			else
			{
				Shader.SetGlobalMatrix(AzureShaderUniforms.StarFieldMatrix, m_starFieldMatrix);
			}
			break;
		case AzureTimeSystem.Realistic:
			m_sunRealisticRotation = GetSunRealisticRotation();
			m_skyController.sunTransform.forward = base.transform.TransformDirection(m_sunRealisticRotation);
			m_moonRealisticRotation = GetMoonRealisticRotation();
			m_skyController.moonTransform.forward = base.transform.TransformDirection(m_moonRealisticRotation);
			m_starFieldOffset.y = longitude;
			m_starFieldMatrix = Matrix4x4.TRS(Vector3.zero, GetCelestialRotation() * Quaternion.Euler(m_skyController.starFieldPosition - m_starFieldOffset), Vector3.one);
			if (m_skyController.shaderUpdateMode == AzureShaderUpdateMode.ByMaterial)
			{
				m_skyController.skyMaterial.SetMatrix(AzureShaderUniforms.StarFieldMatrix, m_starFieldMatrix.inverse);
			}
			else
			{
				Shader.SetGlobalMatrix(AzureShaderUniforms.StarFieldMatrix, m_starFieldMatrix.inverse);
			}
			break;
		}
		m_sunLocalDirection = base.transform.InverseTransformDirection(m_skyController.sunTransform.forward);
		m_moonLocalDirection = base.transform.InverseTransformDirection(m_skyController.moonTransform.forward);
		if (m_skyController.shaderUpdateMode == AzureShaderUpdateMode.ByMaterial)
		{
			m_skyController.skyMaterial.SetVector(AzureShaderUniforms.SunDirection, -m_sunLocalDirection);
			m_skyController.skyMaterial.SetVector(AzureShaderUniforms.MoonDirection, -m_moonLocalDirection);
			m_skyController.skyMaterial.SetMatrix(AzureShaderUniforms.SunMatrix, m_skyController.sunTransform.worldToLocalMatrix);
			m_skyController.skyMaterial.SetMatrix(AzureShaderUniforms.MoonMatrix, m_skyController.moonTransform.worldToLocalMatrix);
			m_skyController.skyMaterial.SetMatrix(AzureShaderUniforms.UpDirectionMatrix, base.transform.worldToLocalMatrix);
			m_skyController.fogMaterial.SetVector(AzureShaderUniforms.SunDirection, -m_sunLocalDirection);
			m_skyController.fogMaterial.SetVector(AzureShaderUniforms.MoonDirection, -m_moonLocalDirection);
			m_skyController.fogMaterial.SetMatrix(AzureShaderUniforms.SunMatrix, m_skyController.sunTransform.worldToLocalMatrix);
			m_skyController.fogMaterial.SetMatrix(AzureShaderUniforms.MoonMatrix, m_skyController.moonTransform.worldToLocalMatrix);
			m_skyController.fogMaterial.SetMatrix(AzureShaderUniforms.UpDirectionMatrix, base.transform.worldToLocalMatrix);
		}
		else
		{
			Shader.SetGlobalVector(AzureShaderUniforms.SunDirection, -m_sunLocalDirection);
			Shader.SetGlobalVector(AzureShaderUniforms.MoonDirection, -m_moonLocalDirection);
			Shader.SetGlobalMatrix(AzureShaderUniforms.SunMatrix, m_skyController.sunTransform.worldToLocalMatrix);
			Shader.SetGlobalMatrix(AzureShaderUniforms.MoonMatrix, m_skyController.moonTransform.worldToLocalMatrix);
			Shader.SetGlobalMatrix(AzureShaderUniforms.UpDirectionMatrix, base.transform.worldToLocalMatrix);
		}
		m_sunElevation = Vector3.Dot(-m_sunLocalDirection, Vector3.up);
		m_moonElevation = Vector3.Dot(-m_moonLocalDirection, Vector3.up);
		m_skyController.directionalLight.transform.localRotation = Quaternion.LookRotation((m_sunElevation >= 0f) ? m_sunLocalDirection : m_moonLocalDirection);
		m_skyController.timeOfDay = m_timeOfDay;
		m_skyController.sunElevation = m_sunElevation;
		m_skyController.moonElevation = m_moonElevation;
	}

	public void UpdateCalendar()
	{
		m_daysInMonth = DateTime.DaysInMonth(year, month);
		day = Mathf.Clamp(day, 1, m_daysInMonth);
		month = Mathf.Clamp(month, 1, 12);
		year = Mathf.Clamp(year, 0, 9999);
		m_dateTime = new DateTime(year, month, 1);
		m_dayOfWeek = (int)m_dateTime.DayOfWeek;
		selectedCalendarDay = day - 1 + m_dayOfWeek;
		for (int i = 0; i < DayList.Length; i++)
		{
			if (i < m_dayOfWeek || i >= m_dayOfWeek + m_daysInMonth)
			{
				DayList[i] = "";
				continue;
			}
			m_dateTime = new DateTime(year, month, i - m_dayOfWeek + 1);
			DayList[i] = m_dateTime.Day.ToString();
		}
	}

	private float GetTimeProgressionStep()
	{
		if (dayLength > 0f)
		{
			return 0.4f / dayLength;
		}
		return 0f;
	}

	public int GetDayOfWeek()
	{
		m_dateTime = new DateTime(year, month, day);
		return (int)m_dateTime.DayOfWeek;
	}

	public int GetDayOfWeek(int year, int month, int day)
	{
		m_dateTime = new DateTime(year, month, day);
		return (int)m_dateTime.DayOfWeek;
	}

	public string GetDayOfWeekString()
	{
		m_dateTime = new DateTime(year, month, day);
		return WeekList[(int)m_dateTime.DayOfWeek];
	}

	public string GetDayOfWeekString(int year, int month, int day)
	{
		m_dateTime = new DateTime(year, month, day);
		return WeekList[(int)m_dateTime.DayOfWeek];
	}

	public void SetTimeline(float value)
	{
		timeline = value;
	}

	public void SetTimeline(Slider slider)
	{
		timeline = slider.value;
	}

	public void SetTimelineTransitionTime(float transitionTime)
	{
		m_timelineTransitionSpeed = transitionTime;
	}

	public void SetTimelineSourceTransitionTime(float source)
	{
		m_timelineSourceTransitionTime = source;
	}

	public void SetTimelineDestinationTransitionTime(float destination)
	{
		m_timelineDestinationTransitionTime = destination;
	}

	public void StartTimelineTransition(float source, float destination, float transitionTime)
	{
		m_timelineTransitionSpeed = transitionTime;
		m_startTimelineTransitionStep = Time.time;
		m_timelineSourceTransitionTime = source;
		m_timelineDestinationTransitionTime = destination;
		m_isTimelineTransitionInProgress = true;
	}

	public void StartTimelineTransition(float destination, float transitionTime)
	{
		m_timelineTransitionSpeed = transitionTime;
		m_startTimelineTransitionStep = Time.time;
		m_timelineSourceTransitionTime = timeline;
		m_timelineDestinationTransitionTime = destination;
		m_isTimelineTransitionInProgress = true;
	}

	public void StartTimelineTransition(float destination)
	{
		m_startTimelineTransitionStep = Time.time;
		m_timelineSourceTransitionTime = timeline;
		m_timelineDestinationTransitionTime = destination;
		m_isTimelineTransitionInProgress = true;
	}

	public void StartTimelineTransition()
	{
		m_startTimelineTransitionStep = Time.time;
		m_isTimelineTransitionInProgress = true;
	}

	private void DoTimelineTransition(float source, float destination)
	{
		m_timelineTransitionStep = Mathf.Clamp01((Time.time - m_startTimelineTransitionStep) / m_timelineTransitionSpeed);
		timeline = Mathf.Lerp(source, destination, m_timelineTransitionStep);
		if (Mathf.Abs(m_timelineTransitionStep - 1f) <= 0f)
		{
			m_isTimelineTransitionInProgress = false;
		}
	}

	public void SetNewDate(int year, int month, int day)
	{
		this.year = year;
		this.month = month;
		this.day = day;
		UpdateCalendar();
	}

	public void SetNewDay(int day)
	{
		this.day = day;
		UpdateCalendar();
	}

	public void IncreaseDay()
	{
		if (repeatMode != AzureTimeRepeatMode.ByDay)
		{
			day++;
			if (day > m_daysInMonth)
			{
				day = 1;
				IncreaseMonth();
			}
		}
		UpdateCalendar();
	}

	public void DecreaseDay()
	{
		if (repeatMode != AzureTimeRepeatMode.ByDay)
		{
			day--;
			m_previousMonth = ((repeatMode == AzureTimeRepeatMode.ByMonth) ? month : (month - 1));
			if (m_previousMonth < 1)
			{
				m_previousMonth = 12;
			}
			m_previousDaysInMonth = DateTime.DaysInMonth(year, m_previousMonth);
			if (day < 1)
			{
				day = m_previousDaysInMonth;
				DecreaseMonth();
			}
		}
		UpdateCalendar();
	}

	public void SetNewMonth(int month)
	{
		this.month = month;
		UpdateCalendar();
	}

	public void IncreaseMonth()
	{
		if (repeatMode != AzureTimeRepeatMode.ByMonth)
		{
			month++;
			if (month > 12)
			{
				month = 1;
				IncreaseYear();
			}
		}
		UpdateCalendar();
	}

	public void DecreaseMonth()
	{
		if (repeatMode != AzureTimeRepeatMode.ByMonth)
		{
			month--;
			if (month < 1)
			{
				month = 12;
				DecreaseYear();
			}
		}
		UpdateCalendar();
	}

	public void SetNewYear(int year)
	{
		this.year = year;
		UpdateCalendar();
	}

	public void IncreaseYear()
	{
		if (repeatMode != AzureTimeRepeatMode.ByYear)
		{
			year++;
			if (year > 9999)
			{
				year = 0;
			}
		}
		UpdateCalendar();
	}

	public void DecreaseYear()
	{
		if (repeatMode != AzureTimeRepeatMode.ByYear)
		{
			year--;
			if (year < 0)
			{
				year = 9999;
			}
		}
		UpdateCalendar();
	}

	public string GetDateString()
	{
		return MonthList[month - 1] + " " + day.ToString("00") + ", " + year.ToString("0000");
	}

	public string GetDateString(string format)
	{
		m_dateTime = new DateTime(year, month, day);
		return m_dateTime.ToString(format);
	}

	private Quaternion GetSunSimpleRotation()
	{
		return Quaternion.Euler(0f, longitude, 0f - latitude) * Quaternion.Euler((m_timeOfDay + utc) * 360f / 24f - 90f, 180f, 0f);
	}

	public Quaternion GetCelestialRotation()
	{
		return Quaternion.Euler(90f - latitude, 0f, 0f) * Quaternion.Euler(0f, longitude, 0f) * Quaternion.Euler(0f, m_lst * 57.29578f, 0f);
	}

	public Vector3 GetSunRealisticRotation()
	{
		m_radians = (float)Math.PI / 180f;
		m_radLatitude = m_radians * latitude;
		m_sinLatitude = Mathf.Sin(m_radLatitude);
		m_cosLatitude = Mathf.Cos(m_radLatitude);
		float num = m_timeOfDay - utc;
		float num2 = 367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530;
		num2 += num / 24f;
		float num3 = 23.4393f - 3.563E-07f * num2;
		float f = m_radians * num3;
		float num4 = Mathf.Sin(f);
		float num5 = Mathf.Cos(f);
		float num6 = 282.9404f + 4.70935E-05f * num2;
		float num7 = 0.016709f - 1.151E-09f * num2;
		float num8 = 356.047f + 0.98560023f * num2;
		float num9 = m_radians * num8;
		float num10 = Mathf.Sin(num9);
		float num11 = Mathf.Cos(num9);
		float f2 = num9 + num7 * num10 * (1f + num7 * num11);
		float num12 = Mathf.Sin(f2);
		float num13 = Mathf.Cos(f2) - num7;
		float num14 = Mathf.Sqrt(1f - num7 * num7) * num12;
		float num15 = 57.29578f * Mathf.Atan2(num14, num13);
		float num16 = Mathf.Sqrt(num13 * num13 + num14 * num14);
		float f3 = m_radians * (num15 + num6);
		float num17 = Mathf.Sin(f3);
		float num18 = Mathf.Cos(f3);
		float num19 = num16 * num18;
		float num20 = num16 * num17;
		float num21 = num19;
		float num22 = num20 * num5;
		float y = num20 * num4;
		float num23 = Mathf.Atan2(num22, num21);
		float f4 = Mathf.Atan2(y, Mathf.Sqrt(num21 * num21 + num22 * num22));
		float num24 = Mathf.Sin(f4);
		float num25 = Mathf.Cos(f4);
		float num26 = num15 + num6 + 180f;
		float num27 = 15f * num;
		float num28 = num26 + num27;
		float f5 = (m_lst = m_radians * (num28 + longitude)) - num23;
		float num29 = Mathf.Sin(f5);
		float num30 = Mathf.Cos(f5) * num25;
		float num31 = num29 * num25;
		float num32 = num24;
		float x = num30 * m_sinLatitude - num32 * m_cosLatitude;
		float y2 = num31;
		float f6 = num30 * m_cosLatitude + num32 * m_sinLatitude;
		float f7 = Mathf.Atan2(y2, x) + m_radians * 180f;
		float num33 = Mathf.Asin(f6);
		float f8 = 90f * m_radians - num33;
		Vector3 vector = default(Vector3);
		vector.z = Mathf.Sin(f8) * Mathf.Cos(f7);
		vector.x = Mathf.Sin(f8) * Mathf.Sin(f7);
		vector.y = Mathf.Cos(f8);
		return vector * -1f;
	}

	public Vector3 GetMoonRealisticRotation()
	{
		float num = m_timeOfDay - utc;
		float num2 = 367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530;
		num2 += num / 24f;
		float num3 = 23.4393f - 3.563E-07f * num2;
		float f = m_radians * num3;
		float num4 = Mathf.Sin(f);
		float num5 = Mathf.Cos(f);
		float num6 = 125.1228f - 0.05295381f * num2;
		float num7 = 5.1454f;
		float num8 = 318.0634f + 0.16435732f * num2;
		float num9 = 0.0549f;
		float num10 = 115.3654f + 13.064993f * num2;
		float num11 = m_radians * num10;
		float f2 = num11 + num9 * Mathf.Sin(num11) * (1f + num9 * Mathf.Cos(num11));
		float num12 = 60.2666f * (Mathf.Cos(f2) - num9);
		float num13 = 60.2666f * (Mathf.Sqrt(1f - num9 * num9) * Mathf.Sin(f2));
		float num14 = 57.29578f * Mathf.Atan2(num13, num12);
		float num15 = Mathf.Sqrt(num12 * num12 + num13 * num13);
		float f3 = m_radians * (num14 + num8);
		float num16 = Mathf.Sin(f3);
		float num17 = Mathf.Cos(f3);
		float f4 = m_radians * num6;
		float f5 = m_radians * num7;
		float num18 = num15 * (Mathf.Cos(f4) * num17 - Mathf.Sin(f4) * num16 * Mathf.Cos(f5));
		float num19 = num15 * (Mathf.Sin(f4) * num17 + Mathf.Cos(f4) * num16 * Mathf.Cos(f5));
		float num20 = num15 * (num16 * Mathf.Sin(f5));
		float num21 = num18;
		float num22 = num19 * num5 - num20 * num4;
		float y = num19 * num4 + num20 * num5;
		float num23 = Mathf.Atan2(num22, num21);
		float f6 = Mathf.Atan2(y, Mathf.Sqrt(num21 * num21 + num22 * num22));
		float f7 = m_lst - num23;
		float num24 = Mathf.Cos(f7) * Mathf.Cos(f6);
		float num25 = Mathf.Sin(f7) * Mathf.Cos(f6);
		float num26 = Mathf.Sin(f6);
		float x = num24 * m_sinLatitude - num26 * m_cosLatitude;
		float y2 = num25;
		float f8 = num24 * m_cosLatitude + num26 * m_sinLatitude;
		float f9 = Mathf.Atan2(y2, x) + m_radians * 180f;
		float num27 = Mathf.Asin(f8);
		float f10 = 90f * m_radians - num27;
		Vector3 vector = default(Vector3);
		vector.z = Mathf.Sin(f10) * Mathf.Cos(f9);
		vector.x = Mathf.Sin(f10) * Mathf.Sin(f9);
		vector.y = Mathf.Cos(f10);
		return vector * -1f;
	}
}
