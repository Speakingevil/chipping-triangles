using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public class ChipScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable modselect;
    public KMBombInfo info;
    public List<KMSelectable> chips;
    public Transform plate;
    public Renderer prend;
    public Material[] pmats;
    public GameObject[] chipobjs;
    public Renderer[] crends;
    public Material[] cmats;
    public TextMesh[] cbtexts;
    public KMColorblindMode cbmode;

    private float defvolume;
    private int platecol;
    private int[][] grid;
    private int[][] pgrid = new int[2][] { new int[9], new int[9]};
    private int[] falsechips = new int[2];
    private int[] ruleind = new int[3];
    private bool[] eat = new bool[18];
    private KMAudio.KMAudioRef gimmemefuckinchips;
    private bool cb;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleDisabled;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        cb = cbmode.ColorblindModeActive;
        platecol = Random.Range(0, 4);
        int r = 0;
        Debug.LogFormat("[Chipping Triangles #{0}] The plate is {1}.", moduleID, new string[] { "slate grey", "lavender", "pale teal", "hazel wood"}[platecol]);
        grid = ChipGrids.cgs[platecol];
        prend.material = pmats[platecol];
        int[,] coords = new int[2,2];
        coords[0, 0] = Random.Range(0, 8);
        coords[0, 1] = Random.Range(0, 2 * coords[0, 0] + 1) & ~1;
        do
        {
            coords[1, 0] = Random.Range(0, 8);
            coords[1, 1] = Random.Range(0, 2 * coords[1, 0] + 1) & ~1;
        } while (coords[1, 0] == coords[0, 0] && coords[1, 1] == coords[0, 1]);
        int f = 0;
        for(int i = 0; i < 2; i++)
        {
            int n = 0;
            r = Random.Range(0, 9);
            falsechips[i] = r;
            for(int j = 0; j < 3; j++)
                for(int k = 0; k <= 2 * j; k++)
                {
                    pgrid[i][n] = grid[coords[i, 0] + j][coords[i, 1] + k];
                    n++;
                }
            int p = pgrid[i][r];
            f = Random.Range(1, 3);
            pgrid[i][r] += f;
            pgrid[i][r] %= 3;
            ruleind[i] = pgrid[i][r] + f;
            ruleind[i] %= 3;
            for(int j = 0; j < 9; j++)
                crends[9 * i + j].material = cmats[3 * pgrid[i][j] + Random.Range(0, 3)];
            Debug.LogFormat("[Chipping Triangles #{0}] The colours of the chips on the {1} of the plate are:\n[Chipping Triangles #{0}] {2}", moduleID, new string[] { "top", "bottom"}[i], string.Join(", ", pgrid[i].Select(x => new string[] { "Red", "Yellow", "Blue"}[x]).ToArray()));
            Debug.LogFormat("[Chipping Triangles #{0}] The false chip on the {1} of the plate is the {2} chip at position {3}. Its true colour is {4}.", moduleID, new string[] { "top", "bottom" }[i], new string[] { "Red", "Yellow", "Blue" }[pgrid[i][r]], r + 1, new string[] { "Red", "Yellow", "Blue" }[p]);
        }
        ruleind[2] = (ruleind[0] == 1 && ruleind[1] == 1) ? 5 : (ruleind[0] + ruleind[1]);
        Debug.LogFormat("[Chipping Triangles #{0}] The desired chips are {1} flavour.", moduleID, new string[] { "Spicy Salsa", "Jalapeño Cheddar", "Sweet Chilli & Sour Cream", "Cheese & Chive", "Cool Ranch", "Triple Cheese" }[ruleind[2]]);
        for (int i = 0; i < 2; i++)
        {
            switch (ruleind[2])
            {
                case 0:
                    int[][] adj = new int[9][] { new int[1] { 2 }, new int[2] { 2, 5 }, new int[3] { 0, 1, 4 }, new int[2] { 2, 7 }, new int[1] { 5 }, new int[3] { 1, 4, 6 }, new int[2] { 5, 7 }, new int[3] { 3, 6, 8 }, new int[1] { 7 } };
                    for (int j = 0; j < 9; j++)
                        eat[9 * i + j] = adj[j].Select(x => pgrid[i][x]).All(x => pgrid[i][j] != x);
                    break;
                case 1:
                    for (int j = 0; j < 9; j++)
                        eat[9 * i + j] = pgrid[i][j] == (ruleind[i] + f) % 3;
                    break;
                case 2:
                    int[] counts = new int[3];
                    for (int j = 0; j < 3; j++)
                        counts[j] = pgrid[i].Count(x => x == j);
                    for(int j = 0; j < 3; j++)
                        if(counts[j] == counts.Where(x => x > 0).Min())
                        {
                            for (int k = 0; k < 9; k++)
                                if (pgrid[i][k] == j)
                                    eat[9 * i + k] = true;
                        }
                    break;
                case 3:
                    int[] flip = new int[9] { 0, 3, 2, 1, 8, 7, 6, 5, 4 };
                    for (int j = 0; j < 9; j++)
                        eat[9 * i + j] = pgrid[i][j] == pgrid[1 - i][flip[j]];
                    break;
                case 4:
                    counts = new int[3];
                    for (int j = 0; j < 3; j++)
                        counts[j] = pgrid[i].Count(x => x == j);
                    for (int j = 0; j < 3; j++)
                        if (counts[j] == 3)
                        {
                            for (int k = 0; k < 9; k++)
                                if (pgrid[i][k] == j)
                                    eat[9 * i + k] = true;
                        }
                    break;
                default:
                    adj = new int[9][] { new int[1] { 2 }, new int[2] { 2, 5 }, new int[3] { 0, 1, 4 }, new int[2] { 2, 7 }, new int[1] { 5 }, new int[3] { 1, 4, 6 }, new int[2] { 5, 7 }, new int[3] { 3, 6, 8 }, new int[1] { 7 } };
                    for (int j = 0; j < 9; j++)
                        eat[9 * i + j] = adj[j].Select(x => pgrid[i][x]).Contains(1) ^ (pgrid[i][j] == 1);
                    break;
            }
            if (Enumerable.Range(9 * i, 9).All(x => !eat[x]))
                for (int j = 0; j < 9; j++)
                    eat[9 * i + j] = j != r;
            Debug.LogFormat("[Chipping Triangles #{0}] Eat the chips in these positions on the {1} of the plate: {2}", moduleID, new string[] { "top", "bottom"}[i], string.Join(", ", Enumerable.Range(9 * i, 9).Where(x => eat[x]).Select(x => ((x % 9) + 1).ToString()).ToArray()));
        }
        foreach(KMSelectable chip in chips)
        {
            int b = chips.IndexOf(chip);
            chip.OnInteract += delegate ()
            {
                Audio.PlaySoundAtTransform("crumch", transform);
                chipobjs[b].SetActive(false);
                if (eat[b])
                {
                    eat[b] = false;
                    if (eat.All(x => !x))
                        module.HandlePass();
                }
                else
                {
                    Audio.PlaySoundAtTransform("AUGH", transform);
                    module.HandleStrike();
                }
                return false;
            };
        }
        if (cb)
            Label(true);
        module.OnActivate += delegate () { StartCoroutine("Funy"); };
    }

    private void Label(bool on)
    {
        if (on)
        {
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 9; j++)
                    cbtexts[9 * i + j].text = "RYB"[pgrid[i][j]].ToString();
            cbtexts[18].text = new string[] { "SLATE GREY", "LAVENDER", "PALE TEAL", "HAZEL WOOD" }[platecol];
        }
        else
            for (int i = 0; i < 19; i++)
                cbtexts[i].text = "";
    }

    private IEnumerator Funy()
    {
        if (!info.GetSolvableModuleNames().Contains("chip") && moduleID == moduleIDCounter)
        {
            if (!Application.isEditor)
            {
                defvolume = GameMusicControl.GameMusicVolume;
                GameMusicControl.GameMusicVolume = 0f;
            }
            gimmemefuckinchips = Audio.PlaySoundAtTransformWithRef("funy", transform);
        }
        float a = -90;
        while (!moduleDisabled)
        {
            a += Time.deltaTime * 240;
            a %= 360;
            plate.localEulerAngles = new Vector3(a, 270, 270);
            plate.localPosition = new Vector3(0, 0.0993f, -0.025f * (Mathf.Sin(a * Mathf.PI / 180) + 1));
            yield return null;
        }
    }

    private void OnDestroy()
    {
        moduleDisabled = true;
        if(gimmemefuckinchips != null)
        {
            if(!Application.isEditor)
                GameMusicControl.GameMusicVolume = defvolume;
            gimmemefuckinchips.StopSound();
            gimmemefuckinchips = null;
        }
    }

#pragma warning disable
    private readonly string TwitchHelpMessage = "!{0} <1-9 / a-i> [Eats chips. Numbers indicate positions on the top of plate (which is shown when the plate as at its highest). Letters indicate positions on the bottom of the plate. Commands can be chained.] | !{0} colourblind [Toggles colourblind mode.]";
#pragma warning restore

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Replace(" ", "");
        if (command == "colourblind" || command == "colorblind")
        {
            cb ^= true;
            Label(cb);
            yield break;
        }
        int[] croonch = new int[command.Length];
        for (int i = 0; i < command.Length; i++)
            if ("123456789".Contains(command[i]))
                croonch[i] = command[i] - '1';
            else if ("abcdefghi".Contains(command[i]))
                croonch[i] = command[i] - 'a' + 9;
            else
            {
                yield return "sendtochaterror!f " + command[i].ToString() + " is not a valid position.";
                yield break;
            }
        if(croonch.Any(x => !chipobjs[x].activeSelf))
        {
            yield return "sendtochaterror!f You tried to eat a chip where there is no chip.";
            yield break;
        }
        for(int i = 0; i < command.Length; i++)
        {
            yield return null;
            chips[croonch[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int[] rand = Enumerable.Range(0, 18).ToArray().Shuffle().ToArray();
        for(int i = 0; i < 18; i++)
        {
            int r = rand[i];
            if (eat[r])
            {
                yield return null;
                chips[r].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
