using System.Runtime.InteropServices;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace ElliotZ;

public static class CameraHelper {
  /// <summary>
  /// 获取相机面向方向的弧度值
  /// </summary>
  public static unsafe float GetCameraRotation() {
    float cameraRotation = ((CameraEx*)CameraManager.Instance() -> GetActiveCamera()) -> DirH;
    int sign = Math.Sign(cameraRotation) == -1 ? -1 : 1;
    return (float)(Math.Abs(cameraRotation) - Math.PI) * sign;
  }
  
  /// <summary>
  /// 获取相机背对方向的弧度值
  /// </summary>
  public static float GetCameraRotationReversed() {
    float result = GetCameraRotation() + (float)Math.PI;

    if (result > Math.PI) {
      result -= (float)(2 * Math.PI);
    } else if (result < -1.0 * Math.PI) {
      result += (float)(2 * Math.PI);
    }

    return result;
  }
  
  /// <summary>
  /// Debug用，获取相机数据的原始Struct
  /// </summary>
  internal static unsafe CameraEx GetCameraExData() {
    CameraEx cameraStats = *(CameraEx*)CameraManager.Instance() -> GetActiveCamera();
    return cameraStats;
  }
  
  /// <summary>
  /// 将世界坐标转换为屏幕坐标。
  /// </summary>
  /// <param name="worldPos">世界坐标位置。</param>
  /// <param name="screenPos">输出的屏幕坐标位置。</param>
  /// <param name="inView">位置是否在视图内。</param>
  /// <returns>位置是否在摄像机前方。</returns>
  public static unsafe bool WorldToScreen(Vector3 worldPos, 
                                          out Vector2 screenPos, 
                                          out bool inView) {
    // 获取当前视口位置、视图投影矩阵和游戏窗口大小
    Vector2 windowPos = ImGuiHelpers.MainViewport.Pos;
    Matrix4x4 viewProjectionMatrix = Control.Instance() -> ViewProjectionMatrix;
    var device = Device.Instance();
    float width = device -> Width;
    float height = device -> Height;

    // 将世界坐标转换为裁剪坐标
    Vector4 pCoords = Vector4.Transform(new Vector4(worldPos, 1.0f), viewProjectionMatrix);
    bool inFront = pCoords.W > 0.0f;

    // 检查位置是否太接近摄像机
    if (Math.Abs(pCoords.W) < float.Epsilon) {
      screenPos = Vector2.Zero;
      inView = false;
      return false;
    }

    // 将裁剪坐标转换为标准化设备坐标
    pCoords *= MathF.Abs(1.0f / pCoords.W);
    screenPos = new Vector2(pCoords.X, pCoords.Y);

    // 将标准化设备坐标转换为屏幕坐标
    screenPos.X = (0.5f * width * (screenPos.X + 1f)) + windowPos.X;
    screenPos.Y = (0.5f * height * (1f - screenPos.Y)) + windowPos.Y;

    // 检查位置是否在视图内
    inView = inFront
          && screenPos.X > windowPos.X
          && screenPos.X < windowPos.X + width
          && screenPos.Y > windowPos.Y
          && screenPos.Y < windowPos.Y + height;

    return inFront;
  }

  /// <summary>
  /// 计算向量位移
  /// </summary>
  /// <param name="position">起始位置</param>
  /// <param name="facingRadians">移动方向，弧度</param>
  /// <param name="distance">移动距离</param>
  /// <returns>终点位置</returns>
  public static Vector3 VectorDisplacement(Vector3 position, float facingRadians, float distance) {
    // 计算 x-z 平面上移动的距离分量
    float dx = (float)(Math.Sin(facingRadians) * distance);
    float dz = (float)(Math.Cos(facingRadians) * distance);

    return new Vector3(position.X + dx, position.Y + 5f, position.Z + dz);
  }
  
  /// <summary>
  /// 计算反向向量位移
  /// </summary>
  /// <param name="position">起始位置</param>
  /// <param name="facingRadians">移动方向，弧度</param>
  /// <param name="distance">移动距离</param>
  /// <returns>终点位置</returns>
  public static Vector3 VectorDisplacementInverse(Vector3 position, float facingRadians, float distance) {
    float dx = (float)(Math.Sin(facingRadians) * -(double)distance);
    float dz = (float)(Math.Cos(facingRadians) * -(double)distance);
    return new Vector3(position.X + dx, position.Y + 5f, position.Z + dz);
  }
}

/// <summary>
/// 7.3的新相机数据Offset
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 688)]
public struct CameraEx
{
  [FieldOffset(320)]
  public float DirH;
  [FieldOffset(324)]
  public float DirV;
  [FieldOffset(328)]
  public float InputDeltaHAdjusted;
  [FieldOffset(332)]
  public float InputDeltaVAdjusted;
  [FieldOffset(336)]
  public float InputDeltaH;
  [FieldOffset(340)]
  public float InputDeltaV;
  [FieldOffset(344)]
  public float DirVMin;
  [FieldOffset(348)]
  public float DirVMax;

  public override string ToString() {
    return $"DirH: {DirH}, DirV: {DirV}, \n"
         + $"InputDeltaHAdjusted: {InputDeltaHAdjusted}, InputDeltaVAdjusted: {InputDeltaVAdjusted},\n"
         + $"InputDeltaH: {InputDeltaH}, InputDeltaV: {InputDeltaV},\n"
         + $"DirVMin: {DirVMin}, DirVMax: {DirVMax}";
  }
}