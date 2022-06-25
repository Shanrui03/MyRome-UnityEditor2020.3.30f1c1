using UnityEditor;
using UnityEngine;
using TMPro;
namespace TMPro.Editor
{

	public class TextMeshProEffectEditor : UnityEditor.Editor
	{
		public override bool RequiresConstantRepaint()
		{
			return true;
		}

		public override void OnInspectorGUI()
		{
			if (!Application.isPlaying)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				SceneView.RepaintAll();
			}
			base.OnInspectorGUI();
		}
	}
}
