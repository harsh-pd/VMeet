using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fordi.UI.MenuControl;

public class CalendarController : MenuScreen
{
    public GameObject _calendarPanel;
    public Text _yearNumText;
    public Text _monthNumText;

    public List<MenuItem> _dateItems = new List<MenuItem>();
    const int _totalDateNum = 42;

    private DateTime _dateTime;
    public static CalendarController _calendarInstance;
    private ITimeForm m_timeForm = null;

    void Start()
    {
        //Debug.LogError("Start");
        _calendarInstance = this;
        Vector3 startPos = m_menuItem.transform.localPosition;
        _dateItems.Clear();
        //_dateItems.Add(m_menuItem);

        for (int i = 0; i < _totalDateNum; i++)
        {
            GameObject item = GameObject.Instantiate(m_menuItem, m_contentRoot) as GameObject;
            item.name = "Item" + (i + 1).ToString();
            //item.transform.SetParent(m_menuItem.transform.parent);
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localPosition = new Vector3((i % 7) * 50 + startPos.x, startPos.y - (i / 7) * 40, startPos.z);

            _dateItems.Add(item.GetComponentInChildren<MenuItem>(true));
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
        index--;

        int date = 0;
        for (int i = 0; i < _totalDateNum; i++)
        {
            MenuItemInfo itemInfo = new MenuItemInfo()
            {
                IsValid = false,
                Text = "",
                Validate = new MenuItemValidationEvent()
            };
            itemInfo.Validate.AddListener((args) => {
                args.IsValid = false;
                args.IsVisible = false;
            });
           

            if (i >= index)
            {
                DateTime thatDay = firstDay.AddDays(date);
                if (thatDay.Month == firstDay.Month)
                {

                    string dateTimeString = string.Empty;
                    var month = _dateTime.Month.ToString();
                    if (month.Length == 1)
                        month = 0 + month;

                    var dateString = "" + (date + 1).ToString();
                    if (dateString.Length == 1)
                        dateString = 0 + dateString;

                    dateTimeString = _dateTime.Year + "-" + month + "-" + dateString + " " + m_timeForm.SelectedTime;

                    DateTime dateTime = DateTime.Now;
                    try
                    {
                        dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(dateTimeString + " " + e.Message);
                    }

                    itemInfo.IsValid = dateTime >= DateTime.Now;
                    itemInfo.Text = (date + 1).ToString();
                    itemInfo.Validate.RemoveAllListeners();
                    itemInfo.Validate.AddListener((args) =>
                    {
                        //Debug.LogError("isVisible: made true");
                        args.IsVisible = true;
                        args.IsValid = dateTime >= DateTime.Now;
                        //Debug.LogError("IsValid: " + args.IsValid + " " + dateTime.ToString() + " " + DateTime.Now.ToString());
                    });
                    date++;
                }
               
            }

            _dateItems[i].Item = itemInfo;
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
        var month = _monthNumText.text;
        if (month.Length == 1)
            month = 0 + month;
        if (day.Length == 1)
            day = 0 + day;
        m_onClick?.Invoke(_yearNumText.text + "-" + month + "-" + day);
        m_vrMenu.CloseLastScreen();
    }

    public override void Init(bool block, bool persist)
    {
        base.Init(block, persist);
        if (block && m_blocker != null)
            m_blocker.gameObject.SetActive(true);
    }

    private Action<string> m_onClick = null;
    public void OpenCalendar(Action<string> OnClick, ITimeForm timeForm)
    {
        m_timeForm = timeForm;
        m_onClick = OnClick;
        Init(true, false);
    }
}
