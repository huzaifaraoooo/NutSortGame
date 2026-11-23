using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    [Header("Bolt Settings")]
    public List<Nut> nuts = new List<Nut>();
    public int maxCapacity = 4;
    public Transform stackPoint;
    public float nutSpacing = 0.45f;

    [Header("Animation Settings")]
    public float totalMoveDuration = 0.35f; // Move hone ka time thora kam kiya
    public float liftHeight = 2.5f;
    public float delayBetweenNuts = 0.05f; // <--- DELAY KAM KAR DIYA HAI (0.15 se 0.05)

    public bool CanPlaceNut(Nut nut)
    {
        if (nuts.Count >= maxCapacity) return false;
        if (nuts.Count == 0) return true;
        return nuts[nuts.Count - 1].nutColor == nut.nutColor;
    }

    public int GetSameColorChainCount(string color)
    {
        int count = 0;
        for (int i = nuts.Count - 1; i >= 0; i--)
        {
            if (nuts[i].nutColor == color)
                count++;
            else
                break;
        }
        return count;
    }

    public void AddNut(Nut nut)
    {
        nuts.Add(nut);
        nut.currentBolt = this;
        nut.transform.SetParent(stackPoint);
        UpdateNutPositions();
    }

    public void RemoveNut(Nut nut)
    {
        nuts.Remove(nut);
        UpdateNutPositions();
    }

    public Nut GetTopNut()
    {
        if (nuts.Count == 0) return null;
        return nuts[nuts.Count - 1];
    }

    public bool IsComplete()
    {
        if (nuts.Count != maxCapacity) return false;
        string color = nuts[0].nutColor;
        foreach (var n in nuts)
            if (n.nutColor != color) return false;
        return true;
    }

    public Vector3 GetBoltTopPoint()
    {
        return stackPoint.position + new Vector3(0, maxCapacity * nutSpacing, 0);
    }

    void UpdateNutPositions()
    {
        for (int i = 0; i < nuts.Count; i++)
        {
            Vector3 targetLocalPos = new Vector3(0, i * nutSpacing, 0);

            if (Vector3.Distance(nuts[i].transform.localPosition, targetLocalPos) < 0.05f)
            {
                nuts[i].transform.localPosition = targetLocalPos;
                nuts[i].transform.localRotation = Quaternion.Euler(80f, 0, 0);
                continue;
            }

            float delay = 0f;
            if (Vector3.Distance(nuts[i].transform.localPosition, targetLocalPos) > 0.5f)
            {
                delay = i * delayBetweenNuts;
            }

            StartCoroutine(MoveNutSquareSmoothly(nuts[i], targetLocalPos, delay));
        }
    }

    IEnumerator MoveNutSquareSmoothly(Nut nut, Vector3 targetLocalPos, float startDelay)
    {
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        Transform nutTransform = nut.transform;

        if (nutTransform.parent == null)
        {
            nutTransform.SetParent(stackPoint);
        }

        Vector3 startPos = nutTransform.position;
        Vector3 targetWorldPos = stackPoint.TransformPoint(targetLocalPos);

        Vector3 topStartPos = new Vector3(startPos.x, startPos.y + liftHeight, startPos.z);
        Vector3 topEndPos = new Vector3(targetWorldPos.x, targetWorldPos.y + liftHeight, targetWorldPos.z);

        float segmentTime = totalMoveDuration / 3f;
        float time = 0;

        // Phase 1: Upar
        while (time < segmentTime)
        {
            time += Time.deltaTime;
            float t = time / segmentTime;
            nutTransform.position = Vector3.Lerp(startPos, topStartPos, t);
            yield return null;
        }
        nutTransform.position = topStartPos;

        // Phase 2: Hawa mein jana + Spin
        time = 0;
        Quaternion startRot = Quaternion.Euler(80f, 0, 0);
        Quaternion targetRot = startRot * Quaternion.Euler(0, 360f, 0);

        while (time < segmentTime)
        {
            time += Time.deltaTime;
            float t = time / segmentTime;
            nutTransform.position = Vector3.Lerp(topStartPos, topEndPos, t);
            nutTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        nutTransform.position = topEndPos;

        // Phase 3: Neechay
        time = 0;
        Quaternion endRot = Quaternion.Euler(80f, 0, 0);
        Quaternion currentRot = nutTransform.rotation;

        while (time < segmentTime)
        {
            time += Time.deltaTime;
            float t = time / segmentTime;
            nutTransform.position = Vector3.Lerp(topEndPos, targetWorldPos, t);
            nutTransform.rotation = Quaternion.Slerp(currentRot, endRot, t);
            yield return null;
        }

        nutTransform.localPosition = targetLocalPos;
        nutTransform.localRotation = Quaternion.Euler(80f, 0, 0);
    }
}