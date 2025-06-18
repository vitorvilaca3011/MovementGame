using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MapSelectorGrid : MonoBehaviour
{
    [System.Serializable]
    public class MapInfo
    {
        public string mapName;
        public string sceneName;
        public Sprite mapPreview;
    }

    public MapInfo[] maps;
    public GameObject mapButtonPrefab;
    public Transform gridParent;

    void Start()
    {
        foreach (MapInfo map in maps)
        {
            GameObject buttonObj = Instantiate(mapButtonPrefab, gridParent);

            // map name text
            TMP_Text nameText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = map.mapName;

            // image preview
            Image previewImage = buttonObj.transform.Find("MapPreview").GetComponent<Image>();
            if (previewImage != null && map.mapPreview != null)
                previewImage.sprite = map.mapPreview;

            // action button
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                string sceneToLoad = map.sceneName; // Precisa copiar pra variável local por causa do closure
                btn.onClick.AddListener(() => SceneManager.LoadScene(sceneToLoad));
            }
        }
    }
}
