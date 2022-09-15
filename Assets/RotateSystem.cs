
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;


//incomplete job, complete its logic!.
[BurstCompile]
public struct RotateJob : IJobParallelFor
{
    public void Execute(int index)
    {
    }
}


public class RotateSystem : MonoBehaviour
{
    public bool UseJob = false;
    public int Size;

    [Range(0, 360f)]
    public float Speed = 15f;
    private NativeArray<Quaternion> m_rotations;
    private JobHandle m_jobHandle;
    private Transform[] m_cubes;

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

    void Update()
    {
        if (UseJob)
        {
            //incomplete job, complete it's logic!.
            var rotJob = new RotateJob
            {

            };
            m_jobHandle = rotJob.Schedule(m_cubes.Length, 1, m_jobHandle);
        }
        else
        {
            for (int i = 0; i < m_cubes.Length; i++)
            {
                var newRot = m_cubes[i].rotation * Quaternion.Euler(0, Time.deltaTime * Speed * 0.2f, 0);
                m_cubes[i].rotation = newRot;
            }
        }
    }

    void LateUpdate()
    {
        //mainthread sync point, you can read native array data now.
        m_jobHandle.Complete();
        if (UseJob)
        {
            for (int i = 0; i < m_cubes.Length; i++)
            {
                m_cubes[i].rotation = m_rotations[i];
            }
        }


    }

}
