using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour {
    
    private Slider health_slider;
    private Slider energy_slider;
    private Slider plasma_1_slider;
    private Slider plasma_2_slider;

    private Slider head_pos_left;
    private Slider head_pos_right;

    private Slider get_slider(string go_name) {
        var go = GameObject.Find(go_name);
        return go.GetComponent<Slider>();
    }

    void Start() {
        health_slider = get_slider("shields");
        energy_slider = get_slider("energy");
        plasma_1_slider = get_slider("plasma_1");
        plasma_2_slider = get_slider("plasma_2");
        head_pos_left = get_slider("head_pos_left");
        head_pos_right = get_slider("head_pos_right");
    }

    public void update_sliders(int health, 
                                int energy, 
                                int plasma_1, 
                                int plasma_2,
                                float head_x) {
        health_slider.value = health;
        energy_slider.value = energy;
        plasma_1_slider.value = plasma_1;
        plasma_2_slider.value = plasma_2;

        if (head_x < 180) {
            float head_val = Mathf.Abs(head_x) * 100f / 50f;
            head_pos_right.value = head_val;
            head_pos_left.value = 0;
        }
        else {
            float head_val = (360 - head_x) * 100f / 60f;
            head_pos_left.value = head_val;
            head_pos_right.value = 0;
        }
         
    }
}
