using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
   private ScreenSpaceOutlines _screenSpaceOutlines;

   private void Start()
   {
      _screenSpaceOutlines = ScriptableObject.CreateInstance<ScreenSpaceOutlines>();
   }
}
