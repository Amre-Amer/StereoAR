using UnityEngine;
using System.Collections.Generic;

public class StereoAR : MonoBehaviour
{
    Vector3 scrPlane;
    public GameObject goThing;
    public GameObject goBall;
    GameObject goParentThings;
    GameObject goParentBalls;
    const int numThings = 50;
    const float distBallMax = 5;
    const float distBallMin = .25f;
    Camera cam;
    Camera camRight;
    List<ThingType> things = new List<ThingType>();
    float distNear = .2f;
    float distIn = 1;
    float distOut = 3;
    float freqBalls = .125f;
    float freqBallsDefault = .125f;
    float freqBallsHello = 1.125f;
    const float distMoveBall = .04f;
    float distMoveThing = .005f;
    AudioSource audioSource;
    bool ynWin;
    public AudioClip[] clipScores;
    public AudioClip clipWin;
    public AudioClip clipOk;
    public AudioClip clipFewLeft;
    public AudioClip clipAgain;
    public AudioClip clipNodYes;
    public AudioClip clipOkMaybeLater;
    public AudioClip[] clipHellos;
    public float pitchCam;
    public float pitchCamLast;
    public int pitchCamDirection;
    public int pitchCamDirectionLast;
    public float yawCam;
    public float yawCamLast;
    public int yawCamDirection;
    public int yawCamDirectionLast;
    const float tolerance = 2f;
    const float delayHello = 15;
    float timeHello;
    GameObject goNods;
    GameObject goNodYes;
    GameObject goNodNo;
    float angNod;
    bool ynFewLeft;

    // Start is called before the first frame update
    void Start()
    {
        goNods = GameObject.Find("Nods");
        goNodNo = GameObject.Find("NodNo");
        goNodYes = GameObject.Find("NodYes");
        audioSource = GetComponent<AudioSource>();
        cam = GameObject.Find("ARCamera").GetComponent<Camera>();
        camRight = GameObject.Find("RightCamera").GetComponent<Camera>();
        goParentThings = new GameObject("parentThings");
        goParentBalls = new GameObject("parentBalls");
        goBall.SetActive(false);
        goThing.SetActive(false);
        goNods.SetActive(false);
        timeHello = Time.realtimeSinceStartup;
        AddBallRepeat();
    }

    private void Update()
    {
        pitchCam = cam.transform.eulerAngles.x;
        yawCam = cam.transform.eulerAngles.y;
        AddThingsRandom();
        UpdateMove();
        UpdateScore();
        UpdateNod();
        CheckNodYes();
        CheckNodNo();
        UpdateHello();
        UpdateResetHello();
        pitchCamLast = pitchCam;
        pitchCamDirectionLast = pitchCamDirection;
        yawCamLast = yawCam;
        yawCamDirectionLast = yawCamDirection;
    }

    void InitNod()
    {
        SetColorBlend(goNodYes.transform.gameObject, Color.green);
        SetColorBlend(goNodNo.transform.gameObject, Color.red);
    }

    void UpdateNod()
    {
        float a = 10 + Mathf.Sin(angNod * Mathf.Deg2Rad) * 15;
        goNodYes.transform.localEulerAngles = new Vector3(a, 0, 0);
        goNodNo.transform.localEulerAngles = new Vector3(0, a, 0);
        angNod += 15;
    }

    void UpdateHello()
    {
        if(Time.realtimeSinceStartup - timeHello > delayHello)
        {
            PlayHello();
            freqBalls = freqBallsHello;
            timeHello = Time.realtimeSinceStartup;
        }
    }

    void UpdateResetHello()
    {

        if (HasChanged(pitchCam, pitchCamLast) == false && HasChanged(yawCam, yawCamLast) == false) return;
        timeHello = Time.realtimeSinceStartup;
        freqBalls = freqBallsDefault;
    }

    void CheckNodYes()
    {
        if (ynWin == false || goNods.activeSelf == false) return;
        if (HasChanged(pitchCam, pitchCamLast) == false) return;
        if (pitchCam > pitchCamLast)
        {
            pitchCamDirection = 1;
        } else
        {
            pitchCamDirection = -1;
        }
        if (pitchCamDirection != pitchCamDirectionLast)
        {
            if (pitchCamDirection == 1)
            {
                Debug.Log("--down\n");
                PlayOk();
                Invoke("ResetThings", 2);
            }
        }
    }

    void CheckNodNo()
    {
        if (ynWin == false || goNods.activeSelf == false) return;
        if (HasChanged(yawCam, yawCamLast) == false) return;
        if (yawCam > yawCamLast)
        {
            yawCamDirection = 1;
        }
        else
        {
            yawCamDirection = -1;
        }
        if (yawCamDirection != yawCamDirectionLast)
        {
            if (yawCamDirection == 1)
            {
                Debug.Log("--left\n");
                PlayOkMaybeLater();
                Invoke("ResetThings", 2);
            }
        }
    }

    void ResetThings()
    {
        Debug.Log("Reset\n");
        goNods.SetActive(false);
        ynFewLeft = false;
        ynWin = false;
        foreach (ThingType thing in things)
        {
            thing.ResetSphereInStart();
        }
    }

    bool HasChanged(float f1, float f2)
    {
        if (Mathf.Abs(f1 - f2) < tolerance)
        {
            return false;
        }
        return true;
    }

    void AddBallRepeat()
    {
        GameObject go = Instantiate(goBall, goParentBalls.transform);
        go.SetActive(true);
        go.transform.position = (cam.transform.position + camRight.transform.position) / 2;
        go.transform.eulerAngles = cam.transform.eulerAngles;
        go.transform.position += go.transform.forward * distBallMin;
        Ball b = go.GetComponent<Ball>();
        b.distMove = distMoveBall;
        Destroy(go, 5);
        Invoke("AddBallRepeat", freqBalls);
    }

    void UpdateMove()
    {
        foreach (ThingType thing in things)
        {
            thing.Move();
        }
    }

    void UpdateScore()
    {
        if (ynWin == true) return;
        int cnt = 0;
        foreach(ThingType thing in things)
        {
            if (thing.ynDone == false)
            {
                foreach (Transform t in goParentBalls.transform)
                {
                    if (IsNearPoint2PointWithDist(thing.go.transform.position, t.position, distNear) == true)
                    {
                        thing.Score();
                    }
                }
                cnt++;
            }
        }
        if (cnt > 0 && cnt < 5 && ynFewLeft == false)
        {
            ynFewLeft = true;
            audioSource.clip = clipFewLeft;
            audioSource.Play();
        }
        if (cnt == 0 && ynWin == false && goNods.activeSelf == false)
        {
            ynWin = true;
            PlayWin();
            goNods.SetActive(true);
            Invoke("PlayAgain", 2);
            Invoke("PlayNodYes", 3.5f);
        }
    }

    void PlayHello()
    {
        int n = Random.Range(0, clipHellos.Length);
        audioSource.clip = clipHellos[n];
        audioSource.Play();
    }

    void PlayOkMaybeLater()
    {
        audioSource.clip = clipOkMaybeLater;
        audioSource.Play();
    }

    void PlayWin()
    {
        audioSource.clip = clipWin;
        audioSource.Play();
    }

    void PlayOk()
    {
        audioSource.clip = clipOk;
        audioSource.Play();
    }

    void PlayAgain()
    {
        audioSource.clip = clipAgain;
        audioSource.Play();
    }

    void PlayNodYes()
    {
        audioSource.clip = clipNodYes;
        audioSource.Play();
    }

    public void PlayRandomScore()
    {
        if (audioSource.isPlaying == false)
        {
            audioSource.clip = GetRandomClipScore();
            audioSource.Play();
        }
    }

    void AddThingsRandom()
    {
        for(int n = 0; n < 5; n++)
        {
            AddThingRandom();
        }
    }

    void AddThingRandom()
    {
        if (things.Count == numThings) return;
        GameObject go = Instantiate(goThing);
        go.SetActive(true);
        go.transform.parent = goParentThings.transform;
        TurnOnOffRenderers(go, true);
        ThingType thing = new ThingType(go, this);
        things.Add(thing);
    }

    void SetColor(GameObject go, Color color)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            rend.material.color = color;
        }
    }

    void SetColorBlend(GameObject go, Color color)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            rend.material.color = (rend.material.color + color) / 2;
        }
    }

    void TurnOnOffRenderers(GameObject go, bool yn)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        foreach(Renderer rend in rends)
        {
            rend.enabled = yn;
        }
    }

    public bool IsNearPoint2PointWithDist(Vector3 pos, Vector3 posCheck, float distCheck)
    {
        float dist = Vector3.Distance(pos, posCheck);
        if (dist < distCheck)
        {
            return true;
        }
        return false;
    }

    AudioClip GetRandomClipScore()
    {
        int n = Random.Range(0, clipScores.Length);
        return clipScores[n];
    }

    class ThingType
    {
        public GameObject go;
        public Vector3 posTarget;
        public int cnt;
        StereoAR g;
        public bool ynDone;
        public ThingType (GameObject go0, StereoAR g0)
        {
            go = go0;
            posTarget = go.transform.position;
            g = g0;
            posTarget = go.transform.transform.position;
            cnt = 0;
        }

        public void Score()
        {
            if (ynDone == true) return;
            Debug.Log("score\n");
            g.PlayRandomScore();
            ynDone = true;
        }

        public void Move()
        {
            if (ynDone == true)
            {
                float scaX = go.transform.localScale.x;
                if (scaX > .1f)
                {
                    scaX *= .95f;
                    go.transform.localScale = Vector3.one * scaX;
                    g.SetColor(go, Random.ColorHSV());
                }
                return;
            }
            if (cnt == 0)
            {
                ResetSphereInStart();
            } 
            if (g.IsNearPoint2PointWithDist(go.transform.position, g.cam.transform.position, g.distIn) == true)
            {
                g.SetColor(go, Color.red);
                return;
            }
            float dist = Vector3.Distance(go.transform.position, g.cam.transform.position);
            float fract = (dist - g.distIn) / (g.distOut - g.distIn);
            Color colorYO = (Color.yellow + Color.red) / 2;
            Color color = colorYO * (1 - fract) + Color.green * fract;
            g.SetColor(go, color);
            go.transform.LookAt(posTarget);
            go.transform.position += go.transform.forward * g.distMoveThing;
            cnt++;
        }

        public void ResetSphereInStart()
        {
            float pitch = Random.Range(-25f, 15f);
            float yaw = Random.Range(0, 360f);
            go.transform.eulerAngles = new Vector3(pitch, yaw, 0);
            go.transform.position = g.cam.transform.position + go.transform.forward * g.distOut;
            posTarget = g.cam.transform.position;
            g.SetColor(go, Color.green);
            ynDone = false;
            go.transform.localScale = Vector3.one;
        }
    }
}
