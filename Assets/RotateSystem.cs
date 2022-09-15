
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct RotateJob : IJobParallelForTransform
{
    public NativeArray<Quaternion> Rotations;
    public NativeArray<float> Offsets;
    public float DeltaTime;
    public float Speed;
    public void Execute(int index, TransformAccess transform)
    {
        var offsetRot = Quaternion.Euler(0, Offsets[index], 0);
        var rot = Rotations[index] * Quaternion.Euler(0, DeltaTime * Speed, 0);

        transform.rotation = rot * offsetRot;
        Rotations[index] = rot;
    }
}


public class RotateSystem : MonoBehaviour
{
    public int Size;
    [Range(0, 360f)]
    public float Speed = 15f;
    private NativeArray<Quaternion> m_rotations;
    private NativeArray<float> m_timeOffsets;
    private Transform[] m_cubes;
    private TransformAccessArray m_transformAccessArray;
    private JobHandle m_jobHandle;

    void Start()
    {
        m_cubes = new Transform[Size * Size];
        m_rotations = new NativeArray<Quaternion>(m_cubes.Length, Allocator.Persistent);
        m_timeOffsets = new NativeArray<float>(m_cubes.Length, Allocator.Persistent);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                var index = i * Size + j;
                m_cubes[index] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                m_cubes[index].position = new Vector3(i * 1.1f, 0, j * 1.1f);
                m_cubes[index].localScale = Vector3.one * 0.5f;
                m_rotations[index] = m_cubes[index].rotation;
                m_timeOffsets[index] = index;
            }
        }
        m_transformAccessArray = new TransformAccessArray(m_cubes);
    }

    void OnDestroy()
    {
        m_transformAccessArray.Dispose();
        m_timeOffsets.Dispose();
        m_rotations.Dispose();
    }

    void Update()
    {
        var rotJob = new RotateJob
        {
            Rotations = m_rotations,
            Offsets = m_timeOffsets,
            DeltaTime = Time.deltaTime,
            Speed = Speed,
        };
        m_jobHandle = rotJob.Schedule(m_transformAccessArray, m_jobHandle);
    }

    void LateUpdate()
    {
        m_jobHandle.Complete();
    }

}
