using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothesController : MonoBehaviour
{
    public GameObject skin_head;
    public GameObject skin_body;

    public GameObject hair_a;
    public GameObject hair_b;
    public GameObject hair_c;
    public GameObject hair_d;
    public GameObject hair_e;

    public GameObject cap;
    public GameObject cap2;
    public GameObject cap3;
    public GameObject chain1;
    public GameObject chain2;
    public GameObject chain3;

    public GameObject mechanic_suit;
    public GameObject mechanic_suit_hat;

    public GameObject nurse_suit;

    public GameObject police_suit;
    public GameObject police_suit_hat;

    public GameObject glasses;
    public GameObject jacket;
    public GameObject pullover;
    public GameObject scarf;
    public GameObject shirt;

    public GameObject shoes1;
    public GameObject shoes2;
    public GameObject shoes3;

    public GameObject shortpants;
    public GameObject t_shirt;
    public GameObject tank_top;
    public GameObject trousers;

    public Texture[] skin_textures;

    public Texture[] beard_textures;

    public Texture[] hair_a_textures;
    public Texture[] hair_b_textures;
    public Texture[] hair_c_textures;
    public Texture[] hair_d_textures;
    public Texture[] hair_e_textures;

    public Texture[] cap_textures;
    public Texture[] cap2_textures;
    public Texture[] cap3_textures;
    public Texture[] chain1_textures;
    public Texture[] chain2_textures;
    public Texture[] chain3_textures;

    public Texture mechanic_suit_texture;
    public Texture nurse_suit_texture;
    public Texture police_suit_texture;

    public Texture[] glasses_texture;
    public Texture[] jacket_textures;
    public Texture[] pullover_textures;
    public Texture[] scarf_textures;
    public Texture[] shirt_textures;

    public Texture[] shoes1_textures;
    public Texture[] shoes2_textures;
    public Texture[] shoes3_textures;

    public Texture[] shortpants_textures;
    public Texture[] t_shirt_textures;
    public Texture[] tank_top_textures;
    public Texture[] trousers_textures;

    bool hat;

    void Start()
    {
        hat = true;

        hair_a.SetActive(false);
        hair_b.SetActive(false);
        hair_c.SetActive(false);
        hair_d.SetActive(false);
        hair_e.SetActive(false);
        cap.SetActive(false);
        cap2.SetActive(false);
        cap3.SetActive(false);
        chain1.SetActive(false);
        chain2.SetActive(false);
        chain3.SetActive(false);
        mechanic_suit.SetActive(false);
        mechanic_suit_hat.SetActive(false);
        nurse_suit.SetActive(false);
        police_suit.SetActive(false);
        police_suit_hat.SetActive(false);
        glasses.SetActive(false);
        jacket.SetActive(false);
        pullover.SetActive(false);
        scarf.SetActive(false);
        shirt.SetActive(false);
        shoes1.SetActive(false);
        shoes2.SetActive(false);
        shoes3.SetActive(false);
        shortpants.SetActive(false);
        t_shirt.SetActive(false);
        tank_top.SetActive(false);
        trousers.SetActive(false);

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
                    if (hair_cut == 0){ hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[0]; }
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
                else if (hairColor == 2)  { hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[2]; }
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

        // determining complet suits or normal cloth
        int suit_or_cloth = UnityEngine.Random.Range(0, 2);

        if (suit_or_cloth == 0)
        {
            int which_suit = UnityEngine.Random.Range(0, 3);
            // mechanicsuit  0
            // nursesuit     1
            // policesuit    2

            // mechanic suit
            if (which_suit == 0)
            {
                mechanic_suit.SetActive(true);

                mechanic_suit.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;
                if (hat)
                {
                    mechanic_suit_hat.SetActive(true);
                    mechanic_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;
                }
            }
            // nurse suit
            if (which_suit == 1)
            {
                nurse_suit.SetActive(true);
                nurse_suit.GetComponent<Renderer>().materials[0].mainTexture = nurse_suit_texture;
            }
            // police suit
            if (which_suit == 2)
            {
                police_suit.SetActive(true);

                police_suit.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;
                if (hat)
                {
                    police_suit_hat.SetActive(true);
                    police_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;
                }
            }
        }

        if (suit_or_cloth == 1)
        {
            // normal cloth

            int shoes = UnityEngine.Random.Range(0, 3);

            if (shoes == 0)
            {
                shoes1.SetActive(true);
                int shoes1_texture = UnityEngine.Random.Range(0, 8);
                shoes1.GetComponent<Renderer>().materials[0].mainTexture = shoes1_textures[shoes1_texture];
            }
            else if (shoes == 1)
            {
                shoes2.SetActive(true);
                int shoes2_texture = UnityEngine.Random.Range(0, 7);
                shoes2.GetComponent<Renderer>().materials[0].mainTexture = shoes2_textures[shoes2_texture];
            }
            else if (shoes == 2)
            {
                shoes3.SetActive(true);
                int shoes3_texture = UnityEngine.Random.Range(0, 6);
                shoes3.GetComponent<Renderer>().materials[0].mainTexture = shoes3_textures[shoes3_texture];
            }


            int glasses_percentage = UnityEngine.Random.Range(0, 100);

            if (glasses_percentage < 20)
            {
                glasses.SetActive(true);
                int texture_choose = UnityEngine.Random.Range(0, 6);
                glasses.GetComponent<Renderer>().materials[0].mainTexture = glasses_texture[texture_choose];
            }

            int chain = UnityEngine.Random.Range(0, 3);

            if (chain == 0)
            {
                chain1.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 4);
                chain1.GetComponent<Renderer>().materials[0].mainTexture = chain1_textures[textures];
            }
            else if (chain == 1)
            {
                chain2.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 3);
                chain2.GetComponent<Renderer>().materials[0].mainTexture = chain2_textures[textures];
            }
            else if (chain == 2)
            {
                chain3.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 3);
                chain3.GetComponent<Renderer>().materials[0].mainTexture = chain3_textures[textures];
            }

            int scarfPercentage = UnityEngine.Random.Range(0, 100);

            if (scarfPercentage < 20)
            {
                scarf.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 11);
                scarf.GetComponent<Renderer>().materials[0].mainTexture = scarf_textures[textures];
            }

            int which_trouser = UnityEngine.Random.Range(0, 2);

            // trousers
            if (which_trouser == 0)
            {
                trousers.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 15);
                trousers.GetComponent<Renderer>().materials[0].mainTexture = trousers_textures[texture];

            }
            else if (which_trouser == 1)
            {
                shortpants.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 11);
                shortpants.GetComponent<Renderer>().materials[0].mainTexture = shortpants_textures[texture];
            }


            // upper bosy cloth :   0 = pullover  1 = shirt    2 = t_shirt    3 = tanktop
            int upper_cloth = UnityEngine.Random.Range(0, 4);


            if (upper_cloth == 0)
            {
                pullover.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 17);
                pullover.GetComponent<Renderer>().materials[0].mainTexture = pullover_textures[texture];
            }
            else if (upper_cloth == 1)
            {
                shirt.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 14);
                shirt.GetComponent<Renderer>().materials[0].mainTexture = shirt_textures[texture];
            }
            else if (upper_cloth == 2)
            {
                t_shirt.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 21);
                t_shirt.GetComponent<Renderer>().materials[0].mainTexture = t_shirt_textures[texture];
            }
            else if (upper_cloth == 3)
            {
                tank_top.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 11);
                tank_top.GetComponent<Renderer>().materials[0].mainTexture = tank_top_textures[texture];
            }
        }
    }
}
