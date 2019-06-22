using UnityEngine;
using TMPro;

namespace Loom.ZombieBattleground
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshProUGUIWithEvents))]
    public class TextMeshProSkew : MonoBehaviour
    {
        public float Skew = 1;

        private TextMeshProUGUIWithEvents _textMeshPro;

        void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUIWithEvents>();
        }

        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            _textMeshPro.TextMeshGenerated += OnTextMeshGenerated;
        }

        void OnDisable()
        {
            _textMeshPro.TextMeshGenerated -= OnTextMeshGenerated;
        }

        private void OnTextMeshGenerated()
        {
            ApplyModification();
        }

        /// <summary>
        /// Method to animate vertex colors of a TMP Text object.
        /// </summary>
        /// <returns></returns>
        void ApplyModification()
        {
            TMP_TextInfo textInfo = _textMeshPro.textInfo;

            int characterCount = textInfo.characterCount;
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * (1f - Skew));

                //destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                //destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}
