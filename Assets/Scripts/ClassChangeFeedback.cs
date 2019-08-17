using UnityEngine;

public class ClassChangeFeedback : MonoBehaviour
{
    public GameObject particleFeedback;
    public Material swordMaterial;
    public Material bowMaterial;
    public Material staffMaterial;

    public void ChangeFeedback(Weapon currentWeapon)
    {
        if (currentWeapon.GetType() == typeof(Sword) || currentWeapon.GetType() == typeof(Dagger))
        {
            ApplyMaterial(swordMaterial, Color.red);
        }
        if (currentWeapon.GetType() == typeof(Bow))
        {
            ApplyMaterial(bowMaterial, Color.cyan);
        }
        if (currentWeapon.GetType() == typeof(Staff))
        {
            ApplyMaterial(staffMaterial, Color.magenta);
        }
    }

    public void ApplyMaterial(Material mat, Color particleColor)
    {
        GetComponentInChildren<SkinnedMeshRenderer>().material = mat;
        var particle = Instantiate(particleFeedback.gameObject, transform.position, transform.rotation);
        var pSystems = particle.GetComponentsInChildren<ParticleSystem>();
        foreach (var sys in pSystems)
        {
            var main = sys.main;
            main.startColor = new ParticleSystem.MinMaxGradient(particleColor);
        }
    }
}