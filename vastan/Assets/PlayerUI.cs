using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour {
    
    private Slider health_slider;
    private Slider energy_slider;
    private Slider plasma_1_slider;
    private Slider plasma_2_slider;

    private Slider get_slider(string go_name) {
        var go = GameObject.Find(go_name);
        return go.GetComponent<Slider>();
    }

    void Start() {
        health_slider = get_slider("shields");
        energy_slider = get_slider("energy");
        plasma_1_slider = get_slider("plasma_1");
        plasma_2_slider = get_slider("plasma_2");
    }

    public void update_sliders(int health, 
                                int energy, 
                                int plasma_1, 
                                int plasma_2) {
        health_slider.value = health;
        energy_slider.value = energy;
        plasma_1_slider.value = plasma_1;
        plasma_2_slider.value = plasma_2;
    }
}
