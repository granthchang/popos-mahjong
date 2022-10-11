using TMPro;
using UnityEngine;

public class NumberField : MonoBehaviour
{
  [SerializeField] private TMP_InputField _input;
  [SerializeField] private int _defaultValue = 0;
  [SerializeField] private int _minValue = 0;
  [SerializeField] private bool _enableMinValue = false;
  [SerializeField] private int _maxValue = 99;
  [SerializeField] private bool _enableMaxValue = false;

  private void Start() {
    _input.text = $"{_defaultValue}";
    if (_minValue > _maxValue) {
      _enableMinValue = _enableMaxValue = false;
    }

    _input.onEndEdit.AddListener(delegate{
      int num = int.Parse(_input.text);
      num = ClampNum(num);
      _input.text = $"{num}";
    });
  }

  public void Increment() {
    int prev = int.Parse(_input.text);
    prev++;
    prev = ClampNum(prev);
    _input.text = $"{prev}";
    _input.onEndEdit?.Invoke(_input.text);
  }

  public void Decrement() {
    int prev = int.Parse(_input.text);
    prev--;
    prev = ClampNum(prev);
    _input.text = $"{prev}";
    _input.onEndEdit?.Invoke(_input.text);
  }

  public int ClampNum(int num) {
    if (_enableMinValue && num < _minValue) {
      return _minValue;
    }
    if (_enableMaxValue && num > _maxValue) {
      return _maxValue;
    }
    return num;
  }
}