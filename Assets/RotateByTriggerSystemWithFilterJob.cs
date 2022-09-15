
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct FetchNonCompeleteTargets : IJobParallelForFilter
{
    public NativeArray<float> Progress;
    public bool Execute(int index)
    {
        return Progress[index] < 1f;
    }
}
[BurstCompile]
public struct RotateTargets : IJobParallelForTransform
{
    public NativeArray<Quaternion> Rotations;
    [WriteOnly]
    public NativeArray<float> Progress;
    public float DeltaTime;
    public float Speed;
    public void Execute(int index, TransformAccess transform)
    {
        var rot = Rotations[index] * Quaternion.Euler(0, DeltaTime * Speed, 0);
        transform.rotation = rot;
        Rotations[index] = rot;
        Progress[index] += DeltaTime;
    }
}

public class RotateByTriggerSystemWithFilterJob : MonoBehaviour
{
    public int Size;
    [Range(0, 360f)]
    public float Speed = 15f;
    private NativeArray<Quaternion> m_rotations;
    private NativeArray<float> m_progress;
    private NativeList<int> m_nonCompeleteTargetIndexes;
    private Transform[] m_cubes;
    private TransformAccessArray m_transformAccessArray;
    private JobHandle m_jobHandle;

    void Start()
    {
        m_cubes = new Transform[Size * Size];
        m_rotations = new NativeArray<Quaternion>(m_cubes.Length, Allocator.Persistent);
        m_progress = new NativeArray<float>(m_cubes.Length, Allocator.Persistent);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                var index = i * Size + j;
                m_cubes[index] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                m_cubes[index].position = new Vector3(i * 1.1f, 0, j * 1.1f);
                m_cubes[index].localScale = Vector3.one * 0.5f;
                m_rotations[index] = m_cubes[index].rotation;
                m_progress[index] = 1f;
            }
        }
        m_transformAccessArray = new TransformAccessArray(m_cubes);
    }

    void OnDestroy()
    {
        m_transformAccessArray.Dispose();
        m_progress.Dispose();
        m_rotations.Dispose();
        if (m_nonCompeleteTargetIndexes.IsCreated)
        {
            m_nonCompeleteTargetIndexes.Dispose();
        }
    }

    private int m_selectedCubeIndex = -1;

    void Update()
    {

        var fetchTargetJob = new FetchNonCompeleteTargets
        {
            Progress = m_progress
        };

        var rotJob = new RotateOnceJob
        {
            Rotations = m_rotations,
            Progress = m_progress,
            DeltaTime = Time.deltaTime,
            Speed = Speed,
        };


        m_nonCompeleteTargetIndexes = new NativeList<int>(Allocator.TempJob);
        m_jobHandle = fetchTargetJob.ScheduleAppend(m_nonCompeleteTargetIndexes, m_cubes.Length, 4, m_jobHandle);
        m_jobHandle = rotJob.Schedule(m_transformAccessArray, m_jobHandle);

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                var objectHit = hit.transform;
                for (int i = 0; i < m_cubes.Length; i++)
                {
                    if (objectHit == m_cubes[i])
                    {
                        m_selectedCubeIndex = i;
                        break;
                    }
                }
            }
        }

    }

    void LateUpdate()
    {
        m_jobHandle.Complete();
        m_nonCompeleteTargetIndexes.Dispose();
        if (m_selectedCubeIndex != -1)
        {
            m_progress[m_selectedCubeIndex] = 0f;
            m_selectedCubeIndex = -1;
        }
    }

}
