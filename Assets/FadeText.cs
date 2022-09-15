using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMPro.TextMeshPro))]
public class FadeText : MonoBehaviour
{
    //half trasnparent sentence ---> "string.Format("<alpha=#{0:X2}>", 255)" + FullTexts
    public string FullTexts;
    private string[] m_alphaTags;
    private float[] m_alphaProgress;
    private bool[] m_fadeIns;
    private int m_index;
    private TextMeshPro m_guiText;
    private float m_elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        m_alphaTags = new string[FullTexts.Length];
        m_alphaProgress = new float[FullTexts.Length];
        m_fadeIns = new bool[FullTexts.Length];
        m_guiText = GetComponent<TextMeshPro>();

        for (int i = 0; i < m_alphaTags.Length; i++)
        {
            m_alphaTags[i] = string.Format("<alpha=#{0:X2}>", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_elapsedTime += Time.deltaTime;
        if (m_index < FullTexts.Length && m_elapsedTime > 0.5f)
        {
            m_fadeIns[m_index] = true;
            m_index++;
            m_elapsedTime = 0f;
        }

        for (int i = 0; i < m_alphaTags.Length; i++)
        {
            if (!m_fadeIns[i])
                break;
            m_alphaProgress[i] = Mathf.Clamp(m_alphaProgress[i] + Time.deltaTime * 255f, 0, 255f);
            m_alphaTags[i] = string.Format("<alpha=#{0:X2}>", (int)m_alphaProgress[i]);
        }

        m_guiText.text = "";
        for (int i = 0; i < FullTexts.Length; i++)
        {
            m_guiText.text += m_alphaTags[i] + FullTexts[i];
        }
    }
}
