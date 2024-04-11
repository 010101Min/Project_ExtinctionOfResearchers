using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceClothesController : MonoBehaviour
{
    public GameObject skin_head;
    public GameObject skin_body;

    public GameObject hair_a;
    public GameObject hair_b;
    public GameObject hair_c;
    public GameObject hair_d;
    public GameObject hair_e;

    public GameObject police_suit;
    public GameObject police_suit_hat;

    public Texture[] skin_textures;

    public Texture[] hair_a_textures;
    public Texture[] hair_b_textures;
    public Texture[] hair_c_textures;
    public Texture[] hair_d_textures;
    public Texture[] hair_e_textures;

    public Texture police_suit_texture;

    bool hat;

    void Start()
    {
        hat = true;

        hair_a.SetActive(false);
        hair_b.SetActive(false);
        hair_c.SetActive(false);
        hair_d.SetActive(false);
        hair_e.SetActive(false);
        police_suit.SetActive(false);
        police_suit_hat.SetActive(false);

        // determining skin color

        int skin_color = UnityEngine.Random.Range(0, 6);

        skin_head.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];
        skin_body.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];

        // determining male or female
        int male_female = UnityEngine.Random.Range(0, 2);

        // determining haircolor
        int hairColor = UnityEngine.Random.Range(0, 4);    // 0 = dark  1 = brown  2 = blonde

        // male
        if (male_female == 0)
        {
            hat = true;

            // choose hair type   hair_a , hair_b  , hair_e
            int hair = UnityEngine.Random.Range(0, 3);

            if (hair == 0)
            {
                hair_a.SetActive(true);
                // 0 = full hair    1 = under cut
                int hair_cut = UnityEngine.Random.Range(0, 2);
                hat = true;

                if (hairColor == 0)
                {
                    if (hair_cut == 0) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[0]; }
                    else if (hair_cut == 1) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[1]; }
                }
                else if (hairColor == 1)
                {
                    if (hair_cut == 0) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[2]; }
                    if (hair_cut == 1) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[3]; }
                }
                else if (hairColor == 2)
                {
                    if (hair_cut == 0) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[4]; }
                    if (hair_cut == 1) { hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[5]; }
                }
            }
            else if (hair == 1)
            {
                hair_b.SetActive(true);
                hat = false;

                // 0 = full hair    1 = under cut
                int hair_cut = UnityEngine.Random.Range(0, 2);

                if (hairColor == 0)
                {
                    if (hair_cut == 0) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[0]; }
                    if (hair_cut == 1) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[5]; }
                }
                if (hairColor == 1)
                {
                    if (hair_cut == 0) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[1]; }
                    if (hair_cut == 1) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[3]; }
                }
                if (hairColor == 2)
                {
                    if (hair_cut == 0) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[2]; }
                    if (hair_cut == 1) { hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[4]; }
                }
            }
            else if (hair == 2)
            {
                hair_e.SetActive(true);
                hat = false;

                if (hairColor == 0) { hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[0]; }
                else if (hairColor == 1) { hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[1]; }
                else if (hairColor == 2) { hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[2]; }
            }
        }
        // female
        if (male_female == 1)
        {
            hat = false;

            // choose hair type   hair_c , hair_d
            int hair = UnityEngine.Random.Range(0, 2);

            if (hair == 0)
            {
                hat = false;
                hair_c.SetActive(true);

                if (hairColor == 0) { hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[0]; }
                else if (hairColor == 1) { hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[1]; }
                else if (hairColor == 2) { hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[2]; }
            }
            if (hair == 1)
            {
                hat = false;
                hair_d.SetActive(true);

                if (hairColor == 0) { hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[0]; }
                else if (hairColor == 1) { hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[1]; }
                else if (hairColor == 2) { hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[2]; }
            }
        }

        police_suit.SetActive(true);

        police_suit.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;
        if (hat)
        {
            police_suit_hat.SetActive(true);
            police_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;
        }
    }
}
