using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    [SerializeField] private DarwinPopulation _population;
    [SerializeField] private TMPro.TMP_Text _generation;
    [SerializeField] private TMPro.TMP_Text _popCount;
    [SerializeField] private TMPro.TMP_Text _heightCount;

    void Update()
    {
        _generation.text = _population.Generation.ToString();
        _popCount.text = _population.PlayerCount.ToString();
        _heightCount.text = _population.BestHeight.ToString();
    }
}
