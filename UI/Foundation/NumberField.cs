using TMPro;
using UnityEngine;

public class NumberField : MonoBehaviour
{
  [SerializeField] private TMP_InputField _inputField;
  public int DefaultValue = 0;
  public int MinValue = 0;
  public bool CalmpMin = false;
  public int MaxValue = 99;
  public bool ClampMax = false;

  private void Start() {
    _inputField.text = $"{DefaultValue}";
    if (MinValue > MaxValue) {
      CalmpMin = ClampMax = false;
    }

    _inputField.onEndEdit.AddListener(delegate{
      int num = int.Parse(_inputField.text);
      num = ClampNum(num);
      _inputField.text = $"{num}";
    });
  }

  public void Increment() {
    int prev = int.Parse(_inputField.text);
    prev++;
    prev = ClampNum(prev);
    _inputField.text = $"{prev}";
    _inputField.onEndEdit?.Invoke(_inputField.text);
  }

  public void Decrement() {
    int prev = int.Parse(_inputField.text);
    prev--;
    prev = ClampNum(prev);
    _inputField.text = $"{prev}";
    _inputField.onEndEdit?.Invoke(_inputField.text);
  }

  private int ClampNum(int num) {
    if (CalmpMin && num < MinValue) {
      return MinValue;
    }
    if (ClampMax && num > MaxValue) {
      return MaxValue;
    }
    return num;
  }
}