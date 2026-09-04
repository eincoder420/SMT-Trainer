using System;
using UnityEngine;

namespace Broccoli.Controller;

[ExecuteInEditMode]
public class BroccoTreeController_1_2_5 : MonoBehaviour
{
	public enum ShaderType
	{
		Standard,
		TreeCreatorOrCompatible,
		SpeedTree7OrCompatible,
		SpeedTree8OrCompatible,
		Billboard
	}

	public enum WindType
	{
		None,
		TreeCreator,
		ST7,
		ST8
	}

	public enum WindQuality
	{
		None,
		Fastest,
		Fast,
		Better,
		Best,
		Palm
	}

	public string version = "";

	public ShaderType shaderType;

	[HideInInspector]
	public bool editorWindAlways;

	private Renderer _renderer;

	private MaterialPropertyBlock _propBlock;

	private Vector4 wind = Vector4.zero;

	[HideInInspector]
	public bool editorWindEnabled;

	public WindType windType;

	public WindQuality windQuality = WindQuality.Better;

	private float baseWindAmplitude = 0.2752f;

	public float localWindAmplitude = 1f;

	public static float globalWindAmplitude;

	public float windMain;

	private float valueTime;

	private Vector4 valueWindDirection = Vector4.zero;

	private Vector4 valueSTWindVector = Vector4.zero;

	private Vector4 valueSTWindGlobal = Vector4.zero;

	private Vector4 valueSTWindBranch = Vector4.zero;

	private Vector4 valueSTWindBranchTwitch = Vector4.zero;

	private Vector4 valueSTWindBranchWhip = Vector4.zero;

	private Vector4 valueSTWindBranchAnchor = Vector4.zero;

	private Vector4 valueSTWindBranchAdherences = Vector4.zero;

	private Vector4 valueSTWindTurbulences = Vector4.zero;

	private Vector4 valueSTWindLeaf1Ripple = Vector4.zero;

	private Vector4 valueSTWindLeaf1Tumble = Vector4.zero;

	private Vector4 valueSTWindLeaf1Twitch = Vector4.zero;

	private Vector4 valueSTWindLeaf2Ripple = Vector4.zero;

	private Vector4 valueSTWindLeaf2Tumble = Vector4.zero;

	private Vector4 valueSTWindLeaf2Twitch = Vector4.zero;

	private Vector4 valueSTWindFrondRipple = Vector4.zero;

	private static int propWindEnabled;

	private static int propWindQuality;

	private static int propSTWindVector;

	private static int propSTWindGlobal;

	private static int propSTWindBranch;

	private static int propSTWindBranchTwitch;

	private static int propSTWindBranchWhip;

	private static int propSTWindBranchAnchor;

	private static int propSTWindBranchAdherences;

	private static int propSTWindTurbulences;

	private static int propSTWindLeaf1Ripple;

	private static int propSTWindLeaf1Tumble;

	private static int propSTWindLeaf1Twitch;

	private static int propSTWindLeaf2Ripple;

	private static int propSTWindLeaf2Tumble;

	private static int propSTWindLeaf2Twitch;

	private static int propSTWindFrondRipple;

	private bool hasSpeedTreeWind
	{
		get
		{
			if (shaderType != ShaderType.SpeedTree7OrCompatible)
			{
				return shaderType == ShaderType.SpeedTree8OrCompatible;
			}
			return true;
		}
	}

	private bool hasTreeCreatorWind => shaderType == ShaderType.TreeCreatorOrCompatible;

	static BroccoTreeController_1_2_5()
	{
		globalWindAmplitude = 1f;
		propWindEnabled = 0;
		propWindQuality = 0;
		propSTWindVector = 0;
		propSTWindGlobal = 0;
		propSTWindBranch = 0;
		propSTWindBranchTwitch = 0;
		propSTWindBranchWhip = 0;
		propSTWindBranchAnchor = 0;
		propSTWindBranchAdherences = 0;
		propSTWindTurbulences = 0;
		propSTWindLeaf1Ripple = 0;
		propSTWindLeaf1Tumble = 0;
		propSTWindLeaf1Twitch = 0;
		propSTWindLeaf2Ripple = 0;
		propSTWindLeaf2Tumble = 0;
		propSTWindLeaf2Twitch = 0;
		propSTWindFrondRipple = 0;
		propWindEnabled = Shader.PropertyToID("_WindEnabled");
		propWindQuality = Shader.PropertyToID("_WindQuality");
		propSTWindVector = Shader.PropertyToID("_ST_WindVector");
		propSTWindVector = Shader.PropertyToID("_ST_WindVector");
		propSTWindGlobal = Shader.PropertyToID("_ST_WindGlobal");
		propSTWindBranch = Shader.PropertyToID("_ST_WindBranch");
		propSTWindBranchTwitch = Shader.PropertyToID("_ST_WindBranchTwitch");
		propSTWindBranchWhip = Shader.PropertyToID("_ST_WindBranchWhip");
		propSTWindBranchAnchor = Shader.PropertyToID("_ST_WindBranchAnchor");
		propSTWindBranchAdherences = Shader.PropertyToID("_ST_WindBranchAdherences");
		propSTWindTurbulences = Shader.PropertyToID("_ST_WindTurbulences");
		propSTWindLeaf1Ripple = Shader.PropertyToID("_ST_WindLeaf1Ripple");
		propSTWindLeaf1Tumble = Shader.PropertyToID("_ST_WindLeaf1Tumble");
		propSTWindLeaf1Twitch = Shader.PropertyToID("_ST_WindLeaf1Twitch");
		propSTWindLeaf2Ripple = Shader.PropertyToID("_ST_WindLeaf2Ripple");
		propSTWindLeaf2Tumble = Shader.PropertyToID("_ST_WindLeaf2Tumble");
		propSTWindLeaf2Twitch = Shader.PropertyToID("_ST_WindLeaf2Twitch");
		propSTWindFrondRipple = Shader.PropertyToID("_ST_WindFrondRipple");
	}

	public void Awake()
	{
	}

	public void Start()
	{
		_renderer = GetComponent<Renderer>();
		if (_renderer != null && shaderType != 0)
		{
			_propBlock = new MaterialPropertyBlock();
			_renderer.GetPropertyBlock(_propBlock);
			if (hasSpeedTreeWind)
			{
				SetupSpeedTreeWind();
			}
			else if (hasTreeCreatorWind)
			{
				SetupTreeCreatorWind();
			}
		}
	}

	private void Update()
	{
		if (_renderer != null && _renderer.isVisible && hasSpeedTreeWind)
		{
			UpdateSpeedTreeWind();
		}
	}

	private void EditorUpdate()
	{
	}

	public void UpdateWind()
	{
		bool enable = windQuality != WindQuality.None;
		if (hasSpeedTreeWind)
		{
			SetupSpeedTreeWind(enable);
		}
		else
		{
			SetupTreeCreatorWind(enable);
		}
	}

	public void SetupTreeCreatorWind(bool enable = true)
	{
		wind = Vector4.zero;
		float num = 1f;
		float num2 = 1f;
		Vector4 zero = Vector4.zero;
		if (enable)
		{
			Vector4 zero2 = Vector4.zero;
			WindZone[] array = UnityEngine.Object.FindObjectsOfType<WindZone>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.activeSelf && array[i].mode == WindZoneMode.Directional)
				{
					zero = new Vector4(array[i].transform.forward.x, array[i].transform.forward.y, array[i].transform.forward.z, 1f);
					num = array[i].windMain * (array[i].windPulseMagnitude + 1f);
					num2 = Mathf.Cos(array[i].windPulseFrequency * (float)Math.PI) * Mathf.Cos(array[i].windPulseFrequency * 3f * (float)Math.PI) * Mathf.Cos(array[i].windPulseFrequency * 5f * (float)Math.PI) + Mathf.Sin(array[i].windPulseFrequency * 25f * (float)Math.PI) * 0.1f;
					num *= num2;
					zero2 = new Vector4(zero.x * num, zero.y * num, zero.z * num, array[i].windTurbulence * (array[i].windPulseMagnitude + 1f));
					wind += zero2;
				}
			}
		}
		if (_propBlock != null)
		{
			_propBlock.SetVector("_Wind", wind);
			_renderer.SetPropertyBlock(_propBlock);
		}
	}

	public void SetupSpeedTreeWind(bool enable = true)
	{
		if (_propBlock != null)
		{
			if (!enable)
			{
				_propBlock.SetFloat(propWindEnabled, 0f);
				_renderer.SetPropertyBlock(_propBlock);
				return;
			}
			GetWindZoneValues();
			_propBlock.SetFloat(propWindEnabled, enable ? 1f : 0f);
			_propBlock.SetFloat(propWindQuality, (float)windQuality);
			valueSTWindVector = valueWindDirection;
			_propBlock.SetVector(propSTWindVector, valueSTWindVector);
			valueSTWindGlobal = new Vector4(Time.time * 0.36f, baseWindAmplitude * localWindAmplitude * globalWindAmplitude * windMain, 0.0655f, 1.728f);
			_propBlock.SetVector(propSTWindGlobal, valueSTWindGlobal);
			valueSTWindBranch = new Vector4(Time.time * 0.65f, 0.4102f, Time.time * 1.5f, 0f);
			_propBlock.SetVector(propSTWindBranch, valueSTWindBranch);
			valueSTWindBranchTwitch = new Vector4(0.603f, 0.147f, 0.75f, 0.3f);
			_propBlock.SetVector(propSTWindBranchTwitch, valueSTWindBranchTwitch);
			valueSTWindBranchWhip = new Vector4(0f, 0f, 0f, 0f);
			_propBlock.SetVector(propSTWindBranchWhip, valueSTWindBranchWhip);
			valueSTWindBranchAnchor = new Vector4(0.034f, 0.4773f, 0.878f, 11.081f);
			_propBlock.SetVector(propSTWindBranchAnchor, valueSTWindBranchAnchor);
			valueSTWindBranchAdherences = new Vector4(0.09295f, 0.1f, 0f, 0f);
			_propBlock.SetVector(propSTWindBranchAdherences, valueSTWindBranchAdherences);
			valueSTWindTurbulences = new Vector4(0.7f, 0.3f, 0f, 0f);
			_propBlock.SetVector(propSTWindTurbulences, valueSTWindTurbulences);
			valueSTWindLeaf1Ripple = new Vector4(Time.time * 3.18f, 0.044f, 0.5f, 0f);
			_propBlock.SetVector(propSTWindLeaf1Ripple, valueSTWindLeaf1Ripple);
			valueSTWindLeaf2Ripple = new Vector4(Time.time * 4.7f, 0f, 0.5f, 0f);
			_propBlock.SetVector(propSTWindLeaf2Ripple, valueSTWindLeaf2Ripple);
			valueSTWindLeaf1Tumble = new Vector4(Time.time * 0.84f, 0.1298f, 0.11403f, 0.11f);
			_propBlock.SetVector(propSTWindLeaf1Tumble, valueSTWindLeaf1Tumble);
			valueSTWindLeaf2Tumble = new Vector4(Time.time, 0.035f, 0.035f, 0.5f);
			_propBlock.SetVector(propSTWindLeaf2Tumble, valueSTWindLeaf2Tumble);
			valueSTWindLeaf1Twitch = new Vector4(0.3315f, 0.3246f, Time.time * 1.56f, 0f);
			_propBlock.SetVector(propSTWindLeaf1Twitch, valueSTWindLeaf1Twitch);
			valueSTWindLeaf2Twitch = new Vector4(0.01745f, 33.3333f, Time.time * 0.31f, 12.896f);
			_propBlock.SetVector(propSTWindLeaf2Twitch, valueSTWindLeaf2Twitch);
			valueSTWindFrondRipple = new Vector4(Time.time * -40.5f, 1.2192f, 10.34f, 0f);
			_propBlock.SetVector(propSTWindFrondRipple, valueSTWindFrondRipple);
			_renderer.SetPropertyBlock(_propBlock);
		}
	}

	private void SetWindQuality(bool enable = true)
	{
		if (shaderType == ShaderType.SpeedTree8OrCompatible)
		{
			Material[] sharedMaterials = _renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				material.DisableKeyword("_WINDQUALITY_NONE");
				material.DisableKeyword("_WINDQUALITY_FASTEST");
				material.DisableKeyword("_WINDQUALITY_FAST");
				material.DisableKeyword("_WINDQUALITY_BETTER");
				material.DisableKeyword("_WINDQUALITY_BEST");
				material.DisableKeyword("_WINDQUALITY_PALM");
				if (enable)
				{
					switch (windQuality)
					{
					case WindQuality.None:
						material.EnableKeyword("_WINDQUALITY_NONE");
						break;
					case WindQuality.Fastest:
						material.EnableKeyword("_WINDQUALITY_FASTEST");
						break;
					case WindQuality.Fast:
						material.EnableKeyword("_WINDQUALITY_FAST");
						break;
					case WindQuality.Better:
						material.EnableKeyword("_WINDQUALITY_BETTER");
						break;
					case WindQuality.Best:
						material.EnableKeyword("_WINDQUALITY_BEST");
						break;
					case WindQuality.Palm:
						material.EnableKeyword("_WINDQUALITY_PALM");
						break;
					}
				}
			}
		}
		else if (shaderType == ShaderType.SpeedTree7OrCompatible)
		{
			Material[] sharedMaterials = _renderer.sharedMaterials;
			foreach (Material material2 in sharedMaterials)
			{
				if (enable)
				{
					material2.EnableKeyword("ENABLE_WIND");
				}
				else
				{
					material2.DisableKeyword("ENABLE_WIND");
				}
			}
		}
		_renderer.GetPropertyBlock(_propBlock);
		_propBlock.SetFloat(propWindEnabled, enable ? 1f : 0f);
		_propBlock.SetFloat(propWindQuality, (float)windQuality);
		_renderer.SetPropertyBlock(_propBlock);
	}

	public void UpdateSpeedTreeWind()
	{
		if (_propBlock != null)
		{
			valueTime = Time.time;
			valueSTWindGlobal.x = valueTime * 0.36f;
			valueSTWindGlobal.y = baseWindAmplitude * localWindAmplitude * globalWindAmplitude * windMain;
			_propBlock.SetVector(propSTWindGlobal, valueSTWindGlobal);
			_renderer.SetPropertyBlock(_propBlock);
			valueSTWindBranch = new Vector4(valueTime * 0.65f, 0.4102f, valueTime * 1.5f, 0f);
			_propBlock.SetVector(propSTWindBranch, valueSTWindBranch);
			valueSTWindLeaf1Ripple = new Vector4(valueTime * 3.18f, 0.044f, 0.5f, 0f);
			_propBlock.SetVector(propSTWindLeaf1Ripple, valueSTWindLeaf1Ripple);
			valueSTWindLeaf2Ripple = new Vector4(valueTime * 4.7f, 0f, 0.5f, 0f);
			_propBlock.SetVector(propSTWindLeaf2Ripple, valueSTWindLeaf2Ripple);
			valueSTWindLeaf1Tumble = new Vector4(valueTime * 0.84f, 0.1298f, 0.11403f, 0.11f);
			_propBlock.SetVector(propSTWindLeaf1Tumble, valueSTWindLeaf1Tumble);
			valueSTWindLeaf2Tumble = new Vector4(valueTime, 0.035f, 0.035f, 0.5f);
			_propBlock.SetVector(propSTWindLeaf2Tumble, valueSTWindLeaf2Tumble);
			valueSTWindLeaf1Twitch = new Vector4(0.3315f, 0.3246f, valueTime * 1.56f, 0f);
			_propBlock.SetVector(propSTWindLeaf1Twitch, valueSTWindLeaf1Twitch);
			valueSTWindLeaf2Twitch = new Vector4(0.01745f, 33.3333f, valueTime * 0.31f, 12.896f);
			_propBlock.SetVector(propSTWindLeaf2Twitch, valueSTWindLeaf2Twitch);
			valueSTWindFrondRipple = new Vector4(valueTime * -40.5f, 1.2192f, 10.34f, 0f);
			_propBlock.SetVector(propSTWindFrondRipple, valueSTWindFrondRipple);
		}
	}

	public void GetWindZoneValues()
	{
		valueWindDirection = new Vector4(1f, 0f, 0f, 0f);
		WindZone[] array = UnityEngine.Object.FindObjectsOfType<WindZone>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.activeSelf && array[i].mode == WindZoneMode.Directional)
			{
				windMain = array[i].windMain;
				valueWindDirection = new Vector4(array[i].transform.forward.x, array[i].transform.forward.y, array[i].transform.forward.z, 1f);
			}
		}
	}

	public void EditorWindAnimate(bool enabled)
	{
	}
}
