using PingCastle.Addition.ReportEnteties;
using PingCastle.Data;
using PingCastle.Healthcheck;
using System;
using System.Collections.Generic;
using System.Text;
using static PingCastle.Report.ReportBase;

namespace PingCastle.Addition.LogicEnteties
{
    public class CustomMethodsReferenceManager
    {
        #region Text
        public AddHeaderTextRef AddHeaderTextRef { get; set; }
        public AddCellTextRef AddCellTextRef { get; set; }
        public AddRef AddRef { get; set; }
        public AddEncodedRef AddEncodedRef { get; set; }
        public AddCellNumRef AddCellNumRef { get; set; }
        public AddGPONameRef AddGPONameRef { get; set; }
        public AddCellNumScoreRef AddCellNumScoreRef { get; set; }
        #endregion

        #region Modal
        public AddBeginModalRef AddBeginModalRef { get; set; }
        public GenerateModalAdminGroupIdFromGroupNameRef GenerateModalAdminGroupIdFromGroupNameRef { get; set; }
        public AddEndModalRef AddEndModalRef { get; set; }

        #endregion

        #region Tooltip
        public AddBeginTooltipRef AddBeginTooltipRef { get; set; }
        public AddEndTooltipRef AddEndTooltipRef { get; set; }

        #endregion

        #region Tables
        public AddBeginTableRef AddBeginTableRef { get; set; }
        public AddBeginTableDataRef AddBeginTableDataRef { get; set; }
        public AddBeginRowRef AddBeginRowRef { get; set; }
        public AddEndRowRef AddEndRowRef { get; set; }
        public AddEndTableRef AddEndTableRef { get; set; }

        #endregion

        #region Sections
        public GenerateSectionRef GenerateSectionRef { get; set; }
        public GenerateSubSectionRef GenerateSubSectionRef { get; set; }

        #endregion

        #region Indicators
        public GenerateSubIndicatorRef GenerateSubIndicatorRef { get; set; }
        public GenerateSubIndicator5ArgsRef GenerateSubIndicator5ArgsRef { get; set; }
        public GenerateAdvancedIndicatorPanelRef GenerateAdvancedIndicatorPanelRef { get; set; }

        #endregion

        #region Paragraph
        public AddParagraphRef AddParagraphRef { get; set; }

        #endregion

        #region Domain

        public AddPrintDomainRef AddPrintDomainRef { get; set; }
        #endregion

        #region Constructor
        public CustomMethodsReferenceManager()
        {

        }

        public CustomMethodsReferenceManager(AddHeaderTextRef addHeaderTextRef, AddCellTextRef addCellTextRef, AddRef addRef, AddGPONameRef addGPONameRef,
            AddEncodedRef addEncodedRef, AddCellNumRef addCellNumRef, AddCellNumScoreRef addCellNumScoreRef,
            AddBeginModalRef addBeginModalRef, GenerateModalAdminGroupIdFromGroupNameRef generateModalAdminGroupIdFromGroupNameRef, AddEndModalRef addEndModalRef,
            AddBeginTooltipRef addBeginTooltipRef, AddEndTooltipRef addEndTooltipRef,
            AddBeginTableRef addBeginTableRef, AddBeginTableDataRef addBeginTableDataRef, AddBeginRowRef addBeginRowRef, AddEndRowRef addEndRowRef, AddEndTableRef addEndTableRef,
            GenerateSectionRef generateSectionRef, GenerateSubSectionRef generateSubSectionRef,
            GenerateSubIndicatorRef generateSubIndicatorRef, GenerateSubIndicator5ArgsRef generateSubIndicator5ArgsRef, GenerateAdvancedIndicatorPanelRef generateAdvancedIndicatorPanelRef,
            AddParagraphRef addParagraphRef,
            AddPrintDomainRef addPrintDomainRef)
        {
            #region Text
            AddHeaderTextRef = addHeaderTextRef;
            AddCellTextRef = addCellTextRef;
            AddRef = addRef;
            AddEncodedRef = addEncodedRef;
            AddCellNumRef = addCellNumRef;
            AddGPONameRef = addGPONameRef;
            AddCellNumScoreRef = addCellNumScoreRef;

            #endregion

            #region Modal
            AddBeginModalRef = addBeginModalRef;
            GenerateModalAdminGroupIdFromGroupNameRef = generateModalAdminGroupIdFromGroupNameRef;
            AddEndModalRef = addEndModalRef;

            #endregion

            #region Table
            AddBeginTableRef = addBeginTableRef;
            AddBeginTableDataRef = addBeginTableDataRef;
            AddBeginRowRef = addBeginRowRef;
            AddEndRowRef = addEndRowRef;
            AddEndTableRef = addEndTableRef;
            #endregion

            #region Tooltip

            AddBeginTooltipRef = addBeginTooltipRef;
            AddEndTooltipRef = addEndTooltipRef;
            #endregion

            #region Section
            GenerateSectionRef = generateSectionRef;
            GenerateSubSectionRef = generateSubSectionRef;
            #endregion

            #region Indicators
            GenerateSubIndicatorRef = generateSubIndicatorRef;
            GenerateSubIndicator5ArgsRef = generateSubIndicator5ArgsRef;
            GenerateAdvancedIndicatorPanelRef = generateAdvancedIndicatorPanelRef;

            #endregion

            #region Paragraph
            AddParagraphRef = addParagraphRef;
            #endregion

            #region Domain

            AddPrintDomainRef = addPrintDomainRef;
            #endregion
        }
        #endregion
    }

    #region Delegates
    public delegate T2 Func<T, T2>(T item);

    #region Text
    public delegate void AddHeaderTextRef(string text, string tooltip = null, bool widetooltip = false);
    public delegate void AddCellTextRef(string text, bool highlight = false, bool IsGood = false, string tooltip = null);
    public delegate void AddRef(string text);
    public delegate void AddEncodedRef(string text);
    public delegate void AddCellNumRef(int num, bool HideIfZero = false);
    public delegate void AddGPONameRef(IGPOReference GPO);
    public delegate void AddCellNumScoreRef(int num);

    #endregion

    #region Modal

    public delegate void AddBeginModalRef(string id, string title, ShowModalType modalType);
    public delegate string GenerateModalAdminGroupIdFromGroupNameRef(string groupname);
    public delegate void AddEndModalRef(ShowModalType modalType = ShowModalType.Normal);
    #endregion

    #region Tooltip
    public delegate void AddBeginTooltipRef(bool wide = false);
    public delegate void AddEndTooltipRef();

    #endregion

    #region Table

    public delegate void AddBeginTableRef(string ariaLabel, bool SimpleTable = false);
    public delegate void AddBeginTableDataRef();
    public delegate void AddBeginRowRef();
    public delegate void AddEndRowRef();
    public delegate void AddEndTableRef(GenerateContentDelegate footer = null);
    #endregion

    #region Section
    public delegate void GenerateSectionRef(string sectionId, string title, GenerateContentDelegate generateContent);
    public delegate void GenerateSubSectionRef(string title, string section = null);

    #endregion

    #region Indicator

    public delegate void GenerateSubIndicatorRef(string category, int globalScore, int score, string explanation);
    public delegate void GenerateSubIndicator5ArgsRef(string category, int globalScore, int score, int numrules, string explanation);
    public delegate void GenerateAdvancedIndicatorPanelRef(string id, string title, string category);
    #endregion

    #region Paragraph

    public delegate void AddParagraphRef(string content);
    #endregion
    #region Domain

    public delegate void AddPrintDomainRef(DomainKey key);
    #endregion

    #endregion
}
