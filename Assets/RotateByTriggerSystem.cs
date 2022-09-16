
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using System.Linq;

[BurstCompile]
public struct RotateOnceJob : IJobFor
{
    public NativeArray<Quaternion> Rotations;
    public float DeltaTime;
    public float Speed;
    public void Execute(int index)
    {
        var rot = Rotations[index] * Quaternion.Euler(0, DeltaTime * Speed, 0);
        Rotations[index] = rot;
    }
}

public class RotateByTriggerSystem : MonoBehaviour
{
    public int Size;
    [Range(0, 360f)]
    public float Speed = 15f;
    //you may requreid a new native array ...............

    private NativeArray<Quaternion> m_rotations;
    private Transform[] m_cubes;
    private JobHandle m_jobHandle;

    void Start()
    {
        m_cubes = new Transform[Size * Size];
        m_rotations = new NativeArray<Quaternion>(m_cubes.Length, Allocator.Persistent);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                var index = i * Size + j;
                m_cubes[index] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                m_cubes[index].position = new Vector3(i * 1.1f, 0, j * 1.1f);
                m_cubes[index].localScale = Vector3.one * 0.5f;
                m_rotations[index] = m_cubes[index].rotation;
            }
        }
    }

    void OnDestroy()
    {
        m_rotations.Dispose();
    }

    private int m_selectedCubeIndex = -1;

    void Update()
    {
        var rotJob = new RotateOnceJob
        {
            Rotations = m_rotations,
            DeltaTime = Time.deltaTime,
            Speed = Speed,
        };
        m_jobHandle = rotJob.ScheduleParallel(m_cubes.Length, 1, m_jobHandle);

        //The logic that can receive selected cube's index
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
        if (m_selectedCubeIndex != -1)
        {
            m_selectedCubeIndex = -1;
        }
        for (int i = 0; i < m_cubes.Length; i++)
        {
            m_cubes[i].rotation = m_rotations[i];
        }
    }

}
