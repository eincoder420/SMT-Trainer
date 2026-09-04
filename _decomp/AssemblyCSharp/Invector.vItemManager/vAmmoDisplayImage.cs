using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager;

public class vAmmoDisplayImage : MonoBehaviour
{
	[Serializable]
	public class vDisplayImage
	{
		public Sprite ammoImage;

		public int ammoId;
	}

	public Image displayImage;

	public Sprite defaultAmmoImage;

	public List<vDisplayImage> displayImages = new List<vDisplayImage>();

	private int currentAmmoId;

	public void ChangeAmmoDisplayImage(int id)
	{
		if (currentAmmoId != id && displayImages != null)
		{
			vDisplayImage vDisplayImage = displayImages.Find((vDisplayImage d) => d.ammoId.Equals(id));
			if (vDisplayImage != null)
			{
				displayImage.sprite = vDisplayImage.ammoImage;
			}
			else
			{
				displayImage.sprite = defaultAmmoImage;
			}
			currentAmmoId = id;
		}
	}
}
