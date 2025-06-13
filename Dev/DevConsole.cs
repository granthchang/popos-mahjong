using System.Reflection;
using UnityEngine;

public class DevConsole : MonoBehaviour {
  [SerializeField] private GameObject _inputField;

  private void Update() {
    if (Input.GetKeyDown(KeyCode.BackQuote)) {
      _inputField.SetActive(!_inputField.activeSelf);
    }
  }

  public void EnterCommand(string input) {
    Debug.Log($">{input}");
    string[] commandArgs = input.Split(" ");
    if (commandArgs.Length == 2) {
      FieldInfo variable = typeof(Constants).GetField(commandArgs[0]);
      if (variable != null) {
        variable.SetValue(this, int.Parse(commandArgs[1]));
        Debug.Log($"Setting \"{commandArgs[0]}\" to {commandArgs[1]}");
      }
      else {
        Debug.Log($"Could not find \"{commandArgs[0]}\"");
      }
    }
    else {
      Debug.Log("Could not recognize command");
    }
  }
}
