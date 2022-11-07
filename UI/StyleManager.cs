using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleManager : MonoBehaviour
{
  [SerializeField] private StyleSettings _styleSettings;
  public static StyleSettings StyleSettings;

  private void Start() {
    StyleSettings = _styleSettings;
  }
}
