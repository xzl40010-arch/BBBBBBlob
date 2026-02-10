using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//2026.2.10 文振一 受伤闪光脚本

public class HurtFlash : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    Coroutine running;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    public void FlashWhite(float duration = 0.05f) // 约 1~2 帧（取决于帧率）
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoFlash(duration));
    }

    private IEnumerator CoFlash(float duration)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = Color.white;

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];

        running = null;
    }
}
