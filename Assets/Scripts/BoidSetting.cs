using System;
using UnityEngine;

[Serializable]
public struct BoidSetting
{
    [Range(0.0f, 5.0f)]
    public float Radius;
    [Range(0.0f, 1.0f)]
    public float Weight;

    public BoidSetting(float radius, float weight)
    {
        Radius = radius;
        Weight = weight;
    }
}


[Serializable]
public struct MeshSetting
{

    public float MeshNo;
    // [Range(0.0f, 5.0f)]
    public float Scale;

    public MeshSetting(float meshNo, float scale)
    {
        MeshNo = meshNo;
        Scale = scale;
    }
}




[Serializable]
public struct GroundWeight
{
    [Range(0.0f, 5.0f)]
    public float FlockingWeight;
    [Range(0.0f, 1.0f)]
    public float DivergeWeight;
    [Range(0.0f, 1.0f)]
    public float CirculationWeight;

    public GroundWeight(float flock, float diverge, float circulate)
    {
        FlockingWeight = flock;
        DivergeWeight = diverge;
        CirculationWeight = circulate;
    }
}



[Serializable]
public struct CeilingWeight
{
    [Range(0.0f, 5.0f)]
    public float FlockingWeight;
    [Range(0.0f, 1.0f)]
    public float ConvergeWeight;
    [Range(0.0f, 1.0f)]
    public float CirculationWeight;

    public CeilingWeight(float flock, float converge, float circulate)
    {
        FlockingWeight = flock;
        ConvergeWeight = converge;
        CirculationWeight = circulate;
    }
}


[Serializable]
public struct RightWallWeight
{
    [Range(0.0f, 5.0f)]
    public float FlockingWeight;
    [Range(0.0f, 1.0f)]
    public float UpMoveWeight;
   
    public RightWallWeight(float flock, float  upmove)
    {
        FlockingWeight = flock;
        UpMoveWeight = upmove;

    }
}
