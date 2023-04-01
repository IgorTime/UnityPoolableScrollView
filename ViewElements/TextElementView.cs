using TMPro;
using UnityEngine;

public class TextElementView : ElementView
{
    [SerializeField]
    private TextMeshProUGUI text;

    public override Vector2 Size => GetComponent<RectTransform>().rect.size;

    public override void Initialize(IElementData data)
    {
        var textData = (TextData) data;
        text.text = textData.Text;
    }
}