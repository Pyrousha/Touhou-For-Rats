using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkSelectables : MonoBehaviour
{
    [Header("This assumes children start top left and go down (and are completely filled, for now)")]
    [SerializeField] private bool startVertical;
    [Space(10)]
    [SerializeField] private int numRows;
    [SerializeField] private int numCols;
    [Space(10)]
    [SerializeField] private bool LinkOnValidate;
    [SerializeField] private List<Selectable> selectables;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (LinkOnValidate)
        {
            Link();
        }
    }
#endif

    public void Link()
    {
        if (startVertical)
            numCols = 0;
        else
            numRows = 0;

        selectables = new List<Selectable>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childObj = transform.GetChild(i);
            if (childObj.gameObject.activeSelf)
                selectables.Add(childObj.GetComponent<Selectable>());
        }

        LinkSpecified(selectables, numRows, numCols);
    }

    public static void LinkSpecified(List<Selectable> _selectables, int _numRows, int _numCols = 1)
    {
        bool startVertical = _numRows > 0;

        _numCols = (int)MathF.Max(_numCols, 1);
        _numRows = (int)MathF.Max(_numRows, 1);

        if (startVertical)
            _numCols = Mathf.CeilToInt((float)_selectables.Count / _numRows);
        else
            _numRows = Mathf.CeilToInt((float)_selectables.Count / _numCols);

        for (int i = 0; i < _selectables.Count; i++)
        {
            Selectable currSelectable = _selectables[i];
            int x;
            int y;
            if (startVertical)
            {
                x = i / _numRows;
                y = i % _numRows;
            }
            else
            {
                x = i % _numCols;
                y = i / _numCols;
            }

            Selectable up = GetSelectableAtPosition(x, y - 1, false, currSelectable);
            Selectable down = GetSelectableAtPosition(x, y + 1, false, currSelectable);
            Selectable left = GetSelectableAtPosition(x - 1, y, true, currSelectable);
            Selectable right = GetSelectableAtPosition(x + 1, y, true, currSelectable);

            Navigation customNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnDown = down,
                selectOnLeft = left,
                selectOnRight = right
            };

            currSelectable.navigation = customNav;
        }

        Selectable GetSelectableAtPosition(int _x, int _y, bool _isHorizontal, Selectable _currSelectable)
        {
            _x = (_x + _numCols) % _numCols;
            _y = (_y + _numRows) % _numRows;
            int newIndex;
            if (startVertical)
                newIndex = _x * _numRows + _y;
            else
                newIndex = _x + _y * _numCols;

            while (newIndex >= _selectables.Count)
            {
                //Invalid
                if (startVertical)
                {
                    if (_isHorizontal)
                        newIndex -= _numRows;
                    else
                        newIndex -= 1;
                }
                else
                {
                    if (_isHorizontal)
                        newIndex -= 1;
                    else
                        newIndex -= _numCols;
                }
            }

            Selectable toReturn = _selectables[newIndex];
            if (toReturn != _currSelectable)
                return toReturn;

            return null;
        }
    }
}

