using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public
class CurveDrawer : MonoBehaviour {

public
  enum Shape {
    Line,
    Circle,
    Cycloid,
    Epicycloid
  }

  public Shape shape;

public
  bool drawCoordinateSystem = true;

public
  float minValue = -100;
public
  float maxValue = 100;

public
  float step = 0.01f;

public
  bool drawOverTime = true;

public
  bool loop = true;

public
  float speed = 3f;

public
  Vector2 point;
public
  Vector2 slope;

public
  float radius = 1;

  [Range(-50, 50)] public float nbCusps = 1;

private
  float time = 0;

  void ResetTime() { time = minValue; }

private
  static int gcd(int a, int b) {
    a = Mathf.Abs(a);
    b = Mathf.Abs(b);

    while (a != 0 && b != 0) {
      if (a > b)
        a %= b;
      else
        b %= a;
    }

    return a == 0 ? b : a;
  }

  // Use this for initialization
  void Start() {
    ResetTime();

    if (shape == Shape.Circle)
      maxValue = minValue + 2 * Mathf.PI;
    else if (shape == Shape.Epicycloid) {
      string fStr = nbCusps.ToString();

      int digitsDec = fStr.Substring(fStr.IndexOf(".") + 1).Length;
      int denom = 1;

      float f = nbCusps;
      for (int i = 0; i < digitsDec; i++) {
        f *= 10;
        denom *= 10;
      }

      int num = (int)Mathf.Round(f);
      denom = denom / gcd(num, denom);

      maxValue = minValue + denom * 2 * Mathf.PI;
    }
  }

  // Update is called once per frame
  void Update() {
    time += Time.deltaTime * speed;
    if (time > maxValue) {
      if (loop)
        ResetTime();
      else
        time = maxValue;
    }
  }

  Vector3 ComputeEquation(float x) {
    if (shape == Shape.Line)
      return new Vector3(slope.x * x + point.x, slope.y * x + point.y);
    else if (shape == Shape.Circle)
      return new Vector3(radius * Mathf.Cos(x), radius * Mathf.Sin(x));
    else if (shape == Shape.Cycloid)
      return new Vector3(radius * (x - Mathf.Sin(x)),
                         radius * (1 - Mathf.Cos(x)));
    else if (shape == Shape.Epicycloid) {
      float k = nbCusps + 1;
      return new Vector3(radius * k * Mathf.Cos(x) - radius * Mathf.Cos(k * x),
                         radius * k * Mathf.Sin(x) - radius * Mathf.Sin(k * x));
    }

    return Vector3.zero;
  }

  void OnDrawGizmos() {
    Gizmos.color = Color.black;

    if (drawCoordinateSystem) {
      Gizmos.DrawLine(Vector3.down * 1000, Vector3.up * 1000);
      Gizmos.DrawLine(Vector3.left * 1000, Vector3.right * 1000);
    }

    Gizmos.color = Color.red;

    Vector3 begin = ComputeEquation(minValue), end = Vector3.zero;

    bool dynamicDraw = (drawOverTime && EditorApplication.isPlaying);

    float limit = dynamicDraw ? time : maxValue;
    for (float x = minValue + step; x <= limit; x += step) {
      end = ComputeEquation(x);

      Gizmos.DrawLine(begin, end);

      begin = end;
    }

    if (dynamicDraw && !(time >= maxValue && !loop)) {
      Gizmos.color = Color.black;
      Gizmos.DrawSphere(ComputeEquation(limit), 3);
    }
  }
}

    [CustomEditor(typeof(CurveDrawer))]
    [CanEditMultipleObjects] public class CurveDrawerEditor
    : UnityEditor.Editor {
  SerializedProperty drawCoordinateSystem;
  SerializedProperty minValue;
  SerializedProperty maxValue;
  SerializedProperty step;
  SerializedProperty drawOverTime;
  SerializedProperty loop;
  SerializedProperty speed;
  SerializedProperty shape;
  SerializedProperty point;
  SerializedProperty slope;
  SerializedProperty radius;
  SerializedProperty nbCusps;

  void OnEnable() {
    drawCoordinateSystem =
        serializedObject.FindProperty("drawCoordinateSystem");
    minValue = serializedObject.FindProperty("minValue");
    maxValue = serializedObject.FindProperty("maxValue");
    step = serializedObject.FindProperty("step");
    drawOverTime = serializedObject.FindProperty("drawOverTime");
    loop = serializedObject.FindProperty("loop");
    speed = serializedObject.FindProperty("speed");
    shape = serializedObject.FindProperty("shape");
    point = serializedObject.FindProperty("point");
    slope = serializedObject.FindProperty("slope");
    radius = serializedObject.FindProperty("radius");
    nbCusps = serializedObject.FindProperty("nbCusps");
  }

public
  override void OnInspectorGUI() {
    serializedObject.Update();

    EditorGUILayout.PropertyField(drawCoordinateSystem);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(minValue);
    EditorGUILayout.PropertyField(maxValue);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(
        step, new GUIContent("Precision", "Lower is better"));

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(drawOverTime);
    if (drawOverTime.boolValue == true) {
      EditorGUILayout.PropertyField(loop);
      EditorGUILayout.PropertyField(speed);
    }

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(shape);
    if (shape.enumValueIndex == (int)CurveDrawer.Shape.Line) {
      EditorGUILayout.PropertyField(point);
      EditorGUILayout.PropertyField(slope);
    } else if (shape.enumValueIndex >= (int)CurveDrawer.Shape.Circle) {
      EditorGUILayout.PropertyField(radius);
      if (shape.enumValueIndex == (int)CurveDrawer.Shape.Epicycloid)
        EditorGUILayout.PropertyField(nbCusps,
                                      new GUIContent("Number of Cusps"));
    }

    serializedObject.ApplyModifiedProperties();
  }
}
