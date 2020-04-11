using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRExperience.UI.MenuControl;

public class CalendarController : MenuScreen
{
    [SerializeField]
    private Blocker m_blocker = null;

    public GameObject _calendarPanel;
    public Text _yearNumText;
    public Text _monthNumText;

    public GameObject _item;

    public List<GameObject> _dateItems = new List<GameObject>();
    const int _totalDateNum = 42;

    private DateTime _dateTime;
    public static CalendarController _calendarInstance;

    void Start()
    {
        _calendarInstance = this;
        Vector3 startPos = _item.transform.localPosition;
        _dateItems.Clear();
        _dateItems.Add(_item);

        for (int i = 1; i < _totalDateNum; i++)
        {
            GameObject item = GameObject.Instantiate(_item, m_contentRoot) as GameObject;
            item.name = "Item" + (i + 1).ToString();
            //item.transform.SetParent(_item.transform.parent);
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localPosition = new Vector3((i % 7) * 50 + startPos.x, startPos.y - (i / 7) * 40, startPos.z);

            _dateItems.Add(item);
        }

        _dateTime = DateTime.Now;

        CreateCalendar();

        _calendarPanel.SetActive(false);
    }

    protected override void AwakeOverride()
    {
        base.AwakeOverride();
        m_blocker.ClickEvent += BlockerClicked;
    }

    protected override void OnDestroyOverride()
    {
        base.OnDestroyOverride();
        m_blocker.ClickEvent -= BlockerClicked;
    }

    private void BlockerClicked(object sender, PointerEventData e)
    {
        m_vrMenu.CloseLastScreen();
    }

    void CreateCalendar()
    {
        DateTime firstDay = _dateTime.AddDays(-(_dateTime.Day - 1));
        int index = GetDays(firstDay.DayOfWeek);

        int date = 0;
        for (int i = 0; i < _totalDateNum; i++)
        {
            TextMeshProUGUI label = _dateItems[i].GetComponentInChildren<TextMeshProUGUI>();
            _dateItems[i].SetActive(false);

            if (i >= index)
            {
                DateTime thatDay = firstDay.AddDays(date);
                if (thatDay.Month == firstDay.Month)
                {
                    _dateItems[i].SetActive(true);

                    label.text = (date + 1).ToString();
                    date++;
                }
            }
        }
        _yearNumText.text = _dateTime.Year.ToString();
        _monthNumText.text = _dateTime.Month.ToString();
    }

    int GetDays(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday: return 1;
            case DayOfWeek.Tuesday: return 2;
            case DayOfWeek.Wednesday: return 3;
            case DayOfWeek.Thursday: return 4;
            case DayOfWeek.Friday: return 5;
            case DayOfWeek.Saturday: return 6;
            case DayOfWeek.Sunday: return 0;
        }

        return 0;
    }
    public void YearPrev()
    {
        _dateTime = _dateTime.AddYears(-1);
        CreateCalendar();
    }

    public void YearNext()
    {
        _dateTime = _dateTime.AddYears(1);
        CreateCalendar();
    }

    public void MonthPrev()
    {
        _dateTime = _dateTime.AddMonths(-1);
        CreateCalendar();
    }

    public void MonthNext()
    {
        _dateTime = _dateTime.AddMonths(1);
        CreateCalendar();
    }

    public void ShowCalendar(Text target)
    {
        _calendarPanel.SetActive(true);
        _target = target;
        _calendarPanel.transform.position = new Vector3(965, 475, 0);//Input.mousePosition-new Vector3(0,120,0);
    }

    Text _target;
    public void OnDateItemClick(string day)
    {
        Debug.LogError("Click");
        m_onClick?.Invoke(_yearNumText.text + "Year" + _monthNumText.text + "Month" + day + "Day");
        m_menuSelection.MeetingDate = _yearNumText.text + "Year" + _monthNumText.text + "Month" + day + "Day";
        //Debug.LogError(_yearNumText.text + "Year" + _monthNumText.text + "Month" + day + "Day");
        m_vrMenu.CloseLastScreen();
    }

    public override void Init(bool block, bool persist)
    {
        base.Init(block, persist);
        if (block && m_blocker != null)
            m_blocker.gameObject.SetActive(true);
    }

    private Action<string> m_onClick = null;
    public void OpenCalendar(Action<string> OnClick)
    {
        m_onClick = OnClick;
    }
}
