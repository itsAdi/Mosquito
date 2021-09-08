using UnityEngine;
using DragonBones;
using System.Collections;
public class handleLizard : MonoBehaviour
{
    [HideInInspector]
    public mosquito Mosquitoe;
    public static float attackRange = 2f;
    public AudioSource BiteSound;

    private UnityArmatureComponent LizardArmature;
    private static bool LizardActivated = false;

    private bool canAttack;

    void Start()
    {
        singletonManager.Instance.lizardInstance = null;
        singletonManager.Instance.lizardInstance = this;
        LizardArmature = gameObject.GetComponentInChildren<UnityArmatureComponent>();
    }

    void Update()
    {
        if (LizardActivated && canAttack && Mosquitoe != null)
        {
            StartCoroutine(attackAndHold());
        }
        if (singletonManager.Instance.gameOver)
        {
            LizardActivated = false;
        }
    }

    public static bool IsLizardActivated
    {
        get
        {
            return LizardActivated;
        }
    }

    IEnumerator attackAndHold()
    {
        Vector2 dir = (Mosquitoe.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.AngleAxis((Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f), Vector3.forward);
        LizardArmature.animation.Play(null, 1);
        if (!BiteSound.isPlaying)
        {
            BiteSound.Play();
        }
        Mosquitoe.resetMosq();
        if (Mosquitoe.IsMosquito)
        {
            if (Mosquitoe.IsBig)
            {
                singletonManager.Instance.currentScore += 10;
            }
            else
            {
                singletonManager.Instance.currentScore += 2;
            }
        }
        canAttack = false;
        Mosquitoe = null;
        yield return new WaitForSeconds(1f); // Wait for one second and make lizard able to attack again
        canAttack = true;
    }

    public IEnumerator enableLizard(Vector2 atPos)
    {
        if (!LizardActivated)
        {
            transform.position = atPos;
            LizardActivated = true;
            canAttack = true;
            yield return new WaitForSeconds(12f); // After five seconds lizard will be disabled
            LizardActivated = false;
            transform.localPosition = Vector2.zero;
        }
    }
}
