using UnityEngine;

public static class ExtensionHandler
{
	public static string Extension(PictureExtension extension)
	{
		return "." + extension.ToString().ToLower();
	}

	public static byte[] ByteArray(Texture2D texture, PictureExtension extension)
	{
		switch (extension)
		{
		case PictureExtension.EXR:
			return texture.EncodeToEXR();
		case PictureExtension.JPG:
			return texture.EncodeToJPG();
		case PictureExtension.PNG:
			return texture.EncodeToPNG();
		case PictureExtension.TGA:
			return texture.EncodeToTGA();
		default:
			Debug.LogError("Not possible to encode 'Texture2D' to byte array ... ");
			return null;
		}
	}
}
