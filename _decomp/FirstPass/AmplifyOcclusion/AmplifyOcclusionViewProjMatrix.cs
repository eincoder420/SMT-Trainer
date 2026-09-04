using UnityEngine;
using UnityEngine.Rendering;

namespace AmplifyOcclusion;

public class AmplifyOcclusionViewProjMatrix
{
	private Matrix4x4 m_prevViewProjMatrixLeft = Matrix4x4.identity;

	private Matrix4x4 m_prevInvViewProjMatrixLeft = Matrix4x4.identity;

	private Matrix4x4 m_prevViewProjMatrixRight = Matrix4x4.identity;

	private Matrix4x4 m_prevInvViewProjMatrixRight = Matrix4x4.identity;

	public void UpdateGlobalShaderConstants_Matrices(CommandBuffer cb, Camera aCamera, bool isUsingTemporalFilter)
	{
		if (AmplifyOcclusionCommon.IsStereoSinglePassEnabled(aCamera))
		{
			Matrix4x4 stereoViewMatrix = aCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoViewMatrix2 = aCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			cb.SetGlobalMatrix(PropertyID._AO_CameraViewLeft, stereoViewMatrix);
			cb.SetGlobalMatrix(PropertyID._AO_CameraViewRight, stereoViewMatrix2);
			Matrix4x4 stereoProjectionMatrix = aCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoProjectionMatrix2 = aCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(stereoProjectionMatrix, renderIntoTexture: false);
			Matrix4x4 gPUProjectionMatrix2 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix2, renderIntoTexture: false);
			cb.SetGlobalMatrix(PropertyID._AO_ProjMatrixLeft, gPUProjectionMatrix);
			cb.SetGlobalMatrix(PropertyID._AO_ProjMatrixRight, gPUProjectionMatrix2);
			if (isUsingTemporalFilter)
			{
				Matrix4x4 matrix4x = gPUProjectionMatrix * stereoViewMatrix;
				Matrix4x4 matrix4x2 = gPUProjectionMatrix2 * stereoViewMatrix2;
				Matrix4x4 matrix4x3 = Matrix4x4.Inverse(matrix4x);
				Matrix4x4 matrix4x4 = Matrix4x4.Inverse(matrix4x2);
				cb.SetGlobalMatrix(PropertyID._AO_InvViewProjMatrixLeft, matrix4x3);
				cb.SetGlobalMatrix(PropertyID._AO_PrevViewProjMatrixLeft, m_prevViewProjMatrixLeft);
				cb.SetGlobalMatrix(PropertyID._AO_PrevInvViewProjMatrixLeft, m_prevInvViewProjMatrixLeft);
				cb.SetGlobalMatrix(PropertyID._AO_InvViewProjMatrixRight, matrix4x4);
				cb.SetGlobalMatrix(PropertyID._AO_PrevViewProjMatrixRight, m_prevViewProjMatrixRight);
				cb.SetGlobalMatrix(PropertyID._AO_PrevInvViewProjMatrixRight, m_prevInvViewProjMatrixRight);
				m_prevViewProjMatrixLeft = matrix4x;
				m_prevInvViewProjMatrixLeft = matrix4x3;
				m_prevViewProjMatrixRight = matrix4x2;
				m_prevInvViewProjMatrixRight = matrix4x4;
			}
		}
		else
		{
			Matrix4x4 worldToCameraMatrix = aCamera.worldToCameraMatrix;
			cb.SetGlobalMatrix(PropertyID._AO_CameraViewLeft, worldToCameraMatrix);
			if (isUsingTemporalFilter)
			{
				Matrix4x4 matrix4x5 = GL.GetGPUProjectionMatrix(aCamera.projectionMatrix, renderIntoTexture: false) * worldToCameraMatrix;
				Matrix4x4 matrix4x6 = Matrix4x4.Inverse(matrix4x5);
				cb.SetGlobalMatrix(PropertyID._AO_InvViewProjMatrixLeft, matrix4x6);
				cb.SetGlobalMatrix(PropertyID._AO_PrevViewProjMatrixLeft, m_prevViewProjMatrixLeft);
				cb.SetGlobalMatrix(PropertyID._AO_PrevInvViewProjMatrixLeft, m_prevInvViewProjMatrixLeft);
				m_prevViewProjMatrixLeft = matrix4x5;
				m_prevInvViewProjMatrixLeft = matrix4x6;
			}
		}
	}
}
