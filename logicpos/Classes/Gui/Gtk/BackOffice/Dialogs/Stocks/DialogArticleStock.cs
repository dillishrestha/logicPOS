﻿using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Gtk;
using logicpos.App;
using logicpos.datalayer.DataLayer.Xpo;
using logicpos.datalayer.Enums;
using logicpos.Classes.Gui.Gtk.Widgets.BackOffice;
using logicpos.Classes.Gui.Gtk.WidgetsGeneric;
using logicpos.Classes.Gui.Gtk.WidgetsXPO;
using logicpos.resources.Resources.Localization;
using System;
using logicpos.Classes.Enums.Dialogs;
using logicpos.Classes.Gui.Gtk.Widgets.Buttons;
using logicpos.Classes.Enums.GenericTreeView;
using logicpos.Classes.Gui.Gtk.Pos.Dialogs;
using System.Data;
using logicpos.datalayer.DataLayer.Xpo.Articles;
using System.Collections;
using static Medsphere.Widgets.GridView;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using logicpos.documentviewer;
using logicpos.financial.library.Classes.Finance;
using logicpos.Classes.Enums.Reports;
using logicpos.Classes.Gui.Gtk.BackOffice.Dialogs.Articles;
using logicpos.financial.library.Classes.Reports;
using System.Threading;

namespace logicpos.Classes.Gui.Gtk.BackOffice
{
    //Gestão de Stocks : Janela de Gestão de Stocks [IN:016534]
    class DialogArticleStock : BOBaseDialog
    {
        //UI
        VBox _vboxTab2;
        TouchButtonIconWithText _openOriginDocumentMovbutton;
        TouchButtonIconWithText _openSellDocumentMovbutton;

        TouchButtonIconWithText _openOriginDocumentbutton;
        TouchButtonIconWithText _openCompositeArticlebutton;
        TouchButtonIconWithText _openChangeArticleLocationbutton;
        TouchButtonIconWithText _openSellDocumentbutton;
        TouchButtonIconWithText _printSerialNumberbutton;
        CriteriaOperator CriteriaOperatorLastFilterStocks;
        CriteriaOperator CriteriaOperatorLastFilterHistory;
        CriteriaOperator CriteriaOperatorLastFilterWarehouse;
        List<fin_articleserialnumber> _listArticleserialnumbers;

        private TouchButtonIconWithText _buttonInsert;
        public TouchButtonIconWithText ButtonInsert
        {
            get { return _buttonInsert; }
            set { _buttonInsert = value; }
        }
        protected GenericTreeViewNavigator<fin_article, TreeViewArticle> _navigator;
        public GenericTreeViewNavigator<fin_article, TreeViewArticle> Navigator
        {
            get { return _navigator; }
            set { _navigator = value; }
        }

        protected GenericTreeViewXPO _treeViewXPO_StockMov;
        public GenericTreeViewXPO TreeViewXPO_StockMov
        {
            get { return _treeViewXPO_StockMov; }
            set { _treeViewXPO_StockMov = value; }
        }

        protected GenericTreeViewXPO _treeViewXPO_ArticleDetails;
        public GenericTreeViewXPO TreeViewXPO_ArticleDetails
        {
            get { return _treeViewXPO_ArticleDetails; }
            set { _treeViewXPO_ArticleDetails = value; }
        }

        protected GenericTreeViewXPO _treeViewXPO_ArticleHistory;
        public GenericTreeViewXPO TreeViewXPO_ArticleHistory
        {
            get { return _treeViewXPO_ArticleHistory; }
            set { _treeViewXPO_ArticleHistory = value; }
        }

        protected GenericTreeViewXPO _treeViewXPO_ArticleWarehouse;
        public GenericTreeViewXPO TreeViewXPO_ArticleWarehouse
        {
            get { return _treeViewXPO_ArticleWarehouse; }
            set { _treeViewXPO_ArticleWarehouse = value; }
        }

        public DialogArticleStock(Window pSourceWindow)
            : base(pSourceWindow, Utils.GetGenericTreeViewXPO<TreeViewArticleStock>(pSourceWindow), DialogFlags.DestroyWithParent, DialogMode.Update, null)
        {
            this.Title = Utils.GetWindowTitle(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "window_title_dialog_document_finance_page3") + " - " + resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_stock_movements"));
            _treeViewXPO_ArticleDetails = Utils.GetGenericTreeViewXPO<TreeViewArticleDetailsStock>(this);
            _treeViewXPO_ArticleHistory = Utils.GetGenericTreeViewXPO<TreeViewArticleSerialNumber>(this, GenericTreeViewNavigatorMode.Default, GenericTreeViewMode.CheckBox);
            _treeViewXPO_ArticleWarehouse = Utils.GetGenericTreeViewXPO<TreeViewArticleWarehouse>(this);
            _treeViewXPO_StockMov = Utils.GetGenericTreeViewXPO<TreeViewArticleStock>(this);
            _listArticleserialnumbers = new List<fin_articleserialnumber>();
            if (GlobalApp.ScreenSize.Width == 800 && GlobalApp.ScreenSize.Height == 600)
            {
                SetSizeRequest(500, 590);
            }
            else
            {
                SetSizeRequest(1200, 700);
            }
            InitUI();
            ShowAll();
        }

        private void InitUI()
        {
            try
            {
                //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                //Tab1 Articles
                VBox vboxTab1 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };

                _treeViewXPO_ArticleDetails.AllowRecordUpdate = false;
                //Format Columns FontSizes for Touch
                _treeViewXPO_ArticleDetails.Navigator.ButtonUpdate.Sensitive = true;
                _treeViewXPO_ArticleDetails.AllowRecordUpdate = true;
                vboxTab1.Add(_treeViewXPO_ArticleDetails);


                //Append Tab
                _notebook.AppendPage(vboxTab1, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "window_title_dialog_document_finance_page3")));

                //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                //Tab2 Stock Moviments
                _vboxTab2 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };
                _vboxTab2.Add(_treeViewXPO_StockMov);
                _openOriginDocumentMovbutton = GetNewButton("touchButtonPrev_DialogActionArea", "Doc. origem", @"Icons/Dialogs/icon_pos_dialog_action_open_document.png");
                _openSellDocumentMovbutton = GetNewButton("touchButtonPrev_DialogActionArea", "Doc. venda", @"Icons/Dialogs/icon_pos_dialog_action_open_document.png");
                if (_treeViewXPO_StockMov != null)
                {
                    _treeViewXPO_StockMov.AllowRecordInsert = true;
                    _treeViewXPO_StockMov.AllowRecordDelete = false;
                    _treeViewXPO_StockMov.Navigator.ButtonInsert.Sensitive = true;
                    _treeViewXPO_StockMov.Navigator.ButtonUpdate.Sensitive = true;
                    _treeViewXPO_StockMov.Navigator.ButtonUpdate.Clicked += ButtonUpdateStockMov_Clicked; ;
                    CriteriaOperatorLastFilterStocks = _treeViewXPO_StockMov.DataSource.Criteria;

                    var _buttonMore = GetNewButton("MoreStocks", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_more"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_more.png");
                    var _buttonFilter = GetNewButton("FilterStocks", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_filter.png");

                    _treeViewXPO_StockMov.Navigator.PackEnd(_openOriginDocumentMovbutton, false, false, 0);
                    _treeViewXPO_StockMov.Navigator.PackEnd(_openSellDocumentMovbutton, false, false, 0);
                    _treeViewXPO_StockMov.Navigator.PackEnd(_buttonMore, false, false, 0);
                    _treeViewXPO_StockMov.Navigator.PackEnd(_buttonFilter, false, false, 0);

                    _buttonMore.Clicked += _buttonMore_Clicked;
                    _buttonFilter.Clicked += _buttonFilter_Clicked;
                    _openOriginDocumentMovbutton.Clicked += _openOriginDocumentMovbutton_Clicked; ;
                    _openSellDocumentMovbutton.Clicked += _openSellDocumentMovbutton_Clicked; ;

                    _treeViewXPO_StockMov.CursorChanged += _treeViewXPO_StockMov_CursorChanged;
                }

                //Append Tab
                _notebook.AppendPage(_vboxTab2, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "report_list_stock_movements")));

                //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                //Tab3 Article History
                _vboxTab2 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };

                _openOriginDocumentbutton = GetNewButton("touchButtonPrev_DialogActionArea", "Doc. origem", @"Icons/Dialogs/icon_pos_dialog_action_open_document.png");
                _openSellDocumentbutton = GetNewButton("touchButtonPrev_DialogActionArea", "Doc. venda", @"Icons/Dialogs/icon_pos_dialog_action_open_document.png");
                _openCompositeArticlebutton = GetNewButton("touchButtonPrev_DialogActionArea", "Composto", @"Icons/Dialogs/icon_pos_dialog_preview.png");
                _openChangeArticleLocationbutton = GetNewButton("touchButtonChange_Location", "Modif. Loc.", @"Icons/icon_pos_nav_refresh.png");
                _printSerialNumberbutton = GetNewButton("touchButtonPrint_SerialNumber", "Cod.Barras", @"Icons/Dialogs/icon_pos_dialog_action_print.png");

                _openOriginDocumentbutton.Sensitive = false;
                _openSellDocumentbutton.Sensitive = false;
                _openCompositeArticlebutton.Sensitive = false;
                _openChangeArticleLocationbutton.Sensitive = false;
                _printSerialNumberbutton.Sensitive = false;

                _openOriginDocumentbutton.Clicked += OpenOriginDocumentbutton_Clicked;
                _openSellDocumentbutton.Clicked += OpenSellDocumentbutton_Clicked;
                _treeViewXPO_ArticleHistory.Navigator.ButtonUpdate.Clicked += OpenCompositeArticlebutton_Clicked;
                _openChangeArticleLocationbutton.Clicked += _openChangeArticleLocationbutton_Clicked;
                _printSerialNumberbutton.Clicked += _printSerialNumberbutton_Clicked;

                _treeViewXPO_ArticleHistory.CursorChanged += _treeViewXPO_ArticleHistory_CursorChanged;
                CriteriaOperatorLastFilterHistory = _treeViewXPO_ArticleHistory.DataSource.Criteria;

                _treeViewXPO_ArticleHistory.Navigator.PackStart(_printSerialNumberbutton, false, false, 0);
                _treeViewXPO_ArticleHistory.Navigator.PackStart(_openOriginDocumentbutton, false, false, 0);
                _treeViewXPO_ArticleHistory.Navigator.PackStart(_openSellDocumentbutton, false, false, 0);
                //_treeViewXPO_ArticleHistory.Navigator.PackStart(_openCompositeArticlebutton, false, false, 0);
                _treeViewXPO_ArticleHistory.Navigator.PackStart(_openChangeArticleLocationbutton, false, false, 0);
                _treeViewXPO_ArticleHistory.Navigator.ButtonDelete.Clicked += _buttonClearSerialNumber_Clicked;

                var _buttonMoreArticles = GetNewButton("MoreHistory", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_more"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_more.png");
                var _buttonFilterArticles = GetNewButton("FilterHistory", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_filter.png");

                _treeViewXPO_ArticleHistory.Navigator.PackEnd(_buttonMoreArticles, false, false, 0);
                _treeViewXPO_ArticleHistory.Navigator.PackEnd(_buttonFilterArticles, false, false, 0);

                _buttonMoreArticles.Clicked += _buttonMore_Clicked;
                _buttonFilterArticles.Clicked += _buttonFilter_Clicked;



                _vboxTab2.Add(_treeViewXPO_ArticleHistory);

                //Append Tab
                _notebook.AppendPage(_vboxTab2, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_article_history")));

                //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

                //Tab4 Warehouse Management
                _vboxTab2 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };
                _vboxTab2.Add(_treeViewXPO_ArticleWarehouse);
                _treeViewXPO_ArticleWarehouse.AllowRecordUpdate = true;
                _treeViewXPO_ArticleWarehouse.AllowRecordInsert = true;
                _treeViewXPO_ArticleWarehouse.AllowRecordDelete = true;
                _treeViewXPO_ArticleWarehouse.Navigator.ButtonInsert.Sensitive = true;
                _treeViewXPO_ArticleWarehouse.Navigator.ButtonDelete.Sensitive = false;
                _treeViewXPO_ArticleWarehouse.Navigator.ButtonUpdate.Sensitive = false;
                _treeViewXPO_ArticleWarehouse.CursorChanged += _treeViewXPO_ArticleWarehouse_CursorChanged;

                CriteriaOperatorLastFilterWarehouse = _treeViewXPO_ArticleWarehouse.DataSource.Criteria;

                var _buttonMoreWarehouse = GetNewButton("MoreWarehouse", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_more"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_more.png");
                var _buttonFilterWarehouse = GetNewButton("FilterWarehouse", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter"), SettingsApp.PaginationRowsPerPage), @"Icons\icon_pos_filter.png");

                _treeViewXPO_ArticleWarehouse.Navigator.PackEnd(_buttonMoreWarehouse, false, false, 0);
                _treeViewXPO_ArticleWarehouse.Navigator.PackEnd(_buttonFilterWarehouse, false, false, 0);

                _buttonMoreWarehouse.Clicked += _buttonMore_Clicked;
                _buttonFilterWarehouse.Clicked += _buttonFilter_Clicked;

                //Append Tab
                _notebook.AppendPage(_vboxTab2, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warehose_management")));

                //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            }
            catch (System.Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _treeViewXPO_StockMov_CursorChanged(object sender, EventArgs e)
        {
            try
            {
                var selectedRow = _treeViewXPO_StockMov.DataSourceRow as fin_articlestock;
                if (selectedRow != null)
                {
                    if (selectedRow.AttachedFile == null) _openOriginDocumentMovbutton.Sensitive = false; else _openOriginDocumentMovbutton.Sensitive = true;
                    if (selectedRow.DocumentMaster == null)
                    {
                        _openSellDocumentMovbutton.Sensitive = false;
                        _treeViewXPO_StockMov.Navigator.ButtonDelete.Sensitive = true;
                        _treeViewXPO_StockMov.Navigator.ButtonUpdate.Sensitive = true;
                        _treeViewXPO_StockMov.AllowRecordDelete = true;
                    }
                    else
                    {
                        _openSellDocumentMovbutton.Sensitive = true;
                        _treeViewXPO_StockMov.Navigator.ButtonUpdate.Sensitive = false;
                        _treeViewXPO_StockMov.Navigator.ButtonDelete.Sensitive = false;
                        _treeViewXPO_StockMov.AllowRecordDelete = false;
                    }
                }
             
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void ButtonUpdateStockMov_Clicked(object sender, EventArgs e)
        {
            var dialog = new DialogArticleStockMoviment(this, _treeViewXPO_StockMov, DialogFlags.DestroyWithParent, _treeViewXPO_StockMov.DataSourceRow as fin_articlestock);

            ResponseType response = (ResponseType)dialog.Run();
            if (response == ResponseType.Ok)
            {
                dialog.Destroy();
            }
            dialog.Destroy();

        }

        private void _openSellDocumentMovbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selecteRow = _treeViewXPO_StockMov.DataSourceRow as fin_articlestock;
                if (selecteRow != null && selecteRow.DocumentMaster != null)
                {
                    var fileToOpen = ProcessFinanceDocument.GenerateDocumentFinanceMasterPDFIfNotExists(selecteRow.DocumentMaster);

                    if (File.Exists(fileToOpen))
                    {
                        if (Utils.IsLinux)
                        {
                            System.Diagnostics.Process.Start(FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, fileToOpen)));
                        }
                        else if (Utils.UsePosPDFViewer() == true && !Utils.IsLinux)
                        {
                            string docPath = FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, fileToOpen));
                            var ScreenSizePDF = GlobalApp.ScreenSize;
                            int widthPDF = ScreenSizePDF.Width;
                            int heightPDF = ScreenSizePDF.Height;
                            System.Windows.Forms.Application.Run(new startDocumentViewer(docPath, widthPDF - 50, heightPDF - 25, false));
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                //Utils.ShowMessageTouch(_sourceWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_error"), "***Cant Update Record ***");
            }
        }

        private void _openOriginDocumentMovbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selecteRow = _treeViewXPO_StockMov.DataSourceRow as fin_articlestock;
                if (selecteRow != null && selecteRow.AttachedFile != null)
                {
                    var fileToOpen = selecteRow.AttachedFile;

                    System.IO.File.WriteAllBytes(selecteRow.DocumentNumber + ".pdf", fileToOpen);

                    if (File.Exists(selecteRow.DocumentNumber + ".pdf"))
                    {
                        if (Utils.IsLinux)
                        {
                            System.Diagnostics.Process.Start(FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, selecteRow.DocumentNumber + ".pdf")));
                        }
                        else if (Utils.UsePosPDFViewer() == true && !Utils.IsLinux)
                        {
                            string docPath = FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, selecteRow.DocumentNumber + ".pdf"));
                            var ScreenSizePDF = GlobalApp.ScreenSize;
                            int widthPDF = ScreenSizePDF.Width;
                            int heightPDF = ScreenSizePDF.Height;
                            System.Windows.Forms.Application.Run(new startDocumentViewer(docPath, widthPDF - 50, heightPDF - 25, false));
                        }
                        File.Delete(selecteRow.DocumentNumber + ".pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _printSerialNumberbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                this.AcceptFocus = false;
                var selectedRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                if (_listArticleserialnumbers.Count > 1)
                {
                    CustomReport.ProcessReportBarcodeLabel(shared.Enums.CustomReportDisplayMode.Print, selectedRow, "", true, _listArticleserialnumbers);
                }
                else
                {
                    CustomReport.ProcessReportBarcodeLabel(shared.Enums.CustomReportDisplayMode.Print, selectedRow, "", true);
                }
                _treeViewXPO_ArticleHistory.Refresh();
                _listArticleserialnumbers.Clear();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _treeViewXPO_ArticleWarehouse_CursorChanged(object sender, EventArgs e)
        {
            try
            {
                var selectedRow = _treeViewXPO_ArticleWarehouse.DataSourceRow as fin_articlewarehouse;
                if (selectedRow != null)
                {
                    _treeViewXPO_ArticleWarehouse.Navigator.ButtonDelete.Sensitive = selectedRow.ArticleSerialNumber == null;
                    _treeViewXPO_ArticleWarehouse.Navigator.ButtonUpdate.Sensitive = selectedRow.ArticleSerialNumber == null ;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _openChangeArticleLocationbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                DialogArticleWarehouse dialog = new DialogArticleWarehouse(this, _treeViewXPO_ArticleWarehouse, DialogFlags.DestroyWithParent, DialogMode.Update, _treeViewXPO_ArticleHistory.DataSourceRow);
                ResponseType response = (ResponseType)dialog.Run();
                if(response == ResponseType.Ok) _treeViewXPO_ArticleHistory.Refresh();

                dialog.Destroy();
               
            }
            catch (System.Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _buttonFilter_Clicked(object sender, EventArgs e)
        {
            try
            {
                string windowTitle = "";
                string tableName = "";
                CriteriaOperator CriteriaOperatorLastFilter = CriteriaOperator.Parse("");
                ReportsQueryDialogMode reportsQueryDialogMode = new ReportsQueryDialogMode();
                GenericTreeViewXPO genericTreeView = new GenericTreeViewXPO();
                if ((sender as TouchButtonIconWithText).Name == "FilterStocks")
                {
                    genericTreeView = _treeViewXPO_StockMov;
                    CriteriaOperatorLastFilter = CriteriaOperatorLastFilterStocks;
                    reportsQueryDialogMode = ReportsQueryDialogMode.FILTER_STOCK_MOVIMENTS;
                    windowTitle = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter") + " " + resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "report_list_stock_movements");
                    tableName = "fin_articlestock";
                }
                else if ((sender as TouchButtonIconWithText).Name == "FilterHistory")
                {
                    genericTreeView = _treeViewXPO_ArticleHistory;
                    CriteriaOperatorLastFilter = CriteriaOperatorLastFilterHistory;
                    reportsQueryDialogMode = ReportsQueryDialogMode.FILTER_ARTICLE_HISTORY;
                    windowTitle = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter") + " " + resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_article_history");
                    tableName = "fin_articleserialnumber";
                }
                else if ((sender as TouchButtonIconWithText).Name == "FilterWarehouse")
                {
                    genericTreeView = _treeViewXPO_ArticleWarehouse;
                    CriteriaOperatorLastFilter = CriteriaOperatorLastFilterWarehouse;
                    reportsQueryDialogMode = ReportsQueryDialogMode.FILTER_ARTICLE_WAREHOUSE;
                    windowTitle = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_button_label_filter") + " " + resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_ConfigurationDevice_PlaceTerminal");
                    tableName = "fin_articlewarehouse";
                }

                // Filter SellDocuments
                string filterField = string.Empty;
                string statusField = string.Empty;
                string extraFilter = string.Empty;

                List<string> result = new List<string>();

                PosReportsQueryDialog dialogFilter = new PosReportsQueryDialog(this, DialogFlags.DestroyWithParent, reportsQueryDialogMode, tableName, windowTitle);
                DialogResponseType responseFilter = (DialogResponseType)dialogFilter.Run();

                //If button Clean Filter Clicked
                if (DialogResponseType.CleanFilter.Equals(responseFilter))
                {
                    genericTreeView.CurrentPageNumber = 1;
                    genericTreeView.DataSource.Criteria = CriteriaOperatorLastFilter;
                    genericTreeView.DataSource.TopReturnedObjects = SettingsApp.PaginationRowsPerPage * genericTreeView.CurrentPageNumber;
                    genericTreeView.Refresh();
                    dialogFilter.Destroy();
                }
                //If OK filter clicked
                else if (DialogResponseType.Ok.Equals(responseFilter))
                {
                    genericTreeView.CurrentPageNumber = 1;
                    filterField = "DocumentType";
                    statusField = "DocumentStatusStatus";

                    //Assign Dialog FilterValue to Method Result Value
                    result.Add($"{dialogFilter.FilterValue}");
                    result.Add(dialogFilter.FilterValueHumanReadble);
                    //string addFilter = FilterValue;

                    if ((sender as TouchButtonIconWithText).Name == "FilterHistory")
                    {
                        result[0] = result[0].Replace("Date", "CreatedAt");
                        result[0] = result[0].Replace("ArticleSerialNumber", "Oid");
                    }
                    //if ((sender as TouchButtonIconWithText).Name == "FilterWarehouse")
                    //{
                    //    //Replace OID for SerialNumber Designation
                    //    if(result[0].Contains("SerialNumber = ")){
                    //        result[0] = result[0].Replace("Date", "CreatedAt");
                    //        int indexSN = result[0].LastIndexOf("SerialNumber = ");
                    //        var splitResult1 = result[0].Substring(indexSN);
                    //        int indexSN2 = splitResult1.LastIndexOf("'");
                    //        string splitResult3 = splitResult1.Remove(indexSN2 + 1);
                    //        var splitResult4 = splitResult3.Substring(splitResult3.IndexOf("'"));
                    //        string finalResult = splitResult4.Remove(splitResult4.Length - 1);
                    //        finalResult = finalResult.Replace("'", "");
                    //        fin_articleserialnumber filterSerialNumber = (fin_articleserialnumber)GlobalFramework.SessionXpo.GetObjectByKey(typeof(fin_articleserialnumber), Guid.Parse(finalResult));
                    //        if(filterSerialNumber != null)
                    //        {
                    //            var newstring = result[0].Replace(finalResult, finalResult);
                    //            result[0] = newstring;
                    //        }
                    //    }
                    //}

                    CriteriaOperator criteriaOperatorLast = genericTreeView.DataSource.Criteria;
                    CriteriaOperator criteriaOperator = GroupOperator.And(CriteriaOperatorLastFilter, CriteriaOperator.Parse(result[0]));

                    //lastData = dialog.GenericTreeView.DataSource;

                    genericTreeView.DataSource.Criteria = criteriaOperator;
                    genericTreeView.DataSource.TopReturnedObjects = SettingsApp.PaginationRowsPerPage * genericTreeView.CurrentPageNumber;
                    genericTreeView.Refresh();

                    //se retornar zero resultados apresenta dados anteriores ao filtro
                    if (genericTreeView.DataSource.Count == 0)
                    {
                        genericTreeView.DataSource.Criteria = criteriaOperatorLast;
                        genericTreeView.DataSource.TopReturnedObjects = SettingsApp.PaginationRowsPerPage * genericTreeView.CurrentPageNumber;
                        genericTreeView.Refresh();
                    }
                    dialogFilter.Destroy();
                }
                else
                {
                    dialogFilter.Destroy();
                }

            }
            catch (System.Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void _buttonMore_Clicked(object sender, EventArgs e)
        {
            try
            {
                GenericTreeViewXPO genericTreeView = new GenericTreeViewXPO();
                if ((sender as TouchButtonIconWithText).Name == "MoreStocks")
                {
                    genericTreeView = _treeViewXPO_StockMov;
                }
                else if ((sender as TouchButtonIconWithText).Name == "MoreHistory")
                {
                    genericTreeView = _treeViewXPO_ArticleHistory;
                }
                else if ((sender as TouchButtonIconWithText).Name == "MoreWarehouse")
                {
                    genericTreeView = _treeViewXPO_ArticleWarehouse;
                }

                genericTreeView.CurrentPageNumber++;
                genericTreeView.DataSource.TopReturnedObjects = (SettingsApp.PaginationRowsPerPage * genericTreeView.CurrentPageNumber);
                genericTreeView.Refresh();
            }
            catch (System.Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

        }

        private void _buttonClearSerialNumber_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selectedRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                if(_listArticleserialnumbers != null && _listArticleserialnumbers.Count > 0)
                {
                    foreach(var item in _listArticleserialnumbers)
                    {
                        DeleteArticleSerialNumber(item);
                    }
                }
                else
                {
                    DeleteArticleSerialNumber(selectedRow);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void DeleteArticleSerialNumber(fin_articleserialnumber pArticleSerialNumber)
        {

            ResponseType responseType = Utils.ShowMessageNonTouch(this, DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "dialog_message_delete_record"), string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warning"), GlobalFramework.ServerVersion));

            var selectedRow = pArticleSerialNumber;

            if (responseType == ResponseType.Yes)
            {

                //Check if is sold
                if (selectedRow.StockMovimentOut != null || selectedRow.IsSold)
                {
                    Utils.ShowMessageNonTouch(this, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "O artigo único já foi vendido", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warning"), GlobalFramework.ServerVersion));
                    return;
                }

                //First check if have references in associted articles
                string sql = string.Format("SELECT Oid FROM [fin_articlecompositionserialnumber] WHERE [ArticleSerialNumberChild] = '{0}';", selectedRow.Oid.ToString());
                var articlecompositionserialnumberOid = selectedRow.Session.ExecuteScalar(sql);
                if (articlecompositionserialnumberOid == null)
                {
                    ////Delete associations
                    //if (selectedRow.ArticleComposition.Count > 0)
                    //{
                    //    selectedRow.Session.Delete(selectedRow.ArticleComposition);
                    //}

                    //add new stock Moviment
                    var articleStock = new fin_articlestock(selectedRow.Session);
                    articleStock.Date = DateTime.Now;
                    articleStock.Article = selectedRow.Article;
                    articleStock.Customer = (erp_customer)selectedRow.Session.GetObjectByKey(typeof(erp_customer), SettingsApp.XpoOidUserRecord);
                    articleStock.DocumentNumber = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_internal_moviment");
                    articleStock.Quantity = -1;
                    articleStock.Save();

                    //Delete Article
                    selectedRow.DeletedAt = DateTime.Now;
                    selectedRow.Disabled = true;
                    var location = (fin_articlewarehouse)selectedRow.Session.GetObjectByKey(typeof(fin_articlewarehouse), selectedRow.ArticleWarehouse.Oid);

                    location.Disabled = true;
                    location.DeletedAt = DateTime.Now;
                    location.Save();

                    selectedRow.SerialNumber = Utils.RandomString();
                    selectedRow.Save();

                    _treeViewXPO_ArticleHistory.Refresh();
                    _treeViewXPO_ArticleWarehouse.Refresh();
                    _treeViewXPO_StockMov.Refresh();

                    _log.Debug("Serial Number deleted");
                }
                else
                {
                    Utils.ShowMessageNonTouch(this, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "O artigo único está associado a outro(s) artigos", string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warning"), GlobalFramework.ServerVersion));
                }
            }


        }

        private void _treeViewXPO_ArticleHistory_CursorChanged(object sender, EventArgs e)
        {
            try
            {
                //Button Sensitive
                var selectedRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                if (selectedRow != null)
                {
                    _printSerialNumberbutton.Sensitive = true;
                    _openChangeArticleLocationbutton.Sensitive = true;
                    //_openCompositeArticlebutton.Sensitive = selectedRow.Article.IsComposed;
                    if (selectedRow.StockMovimentIn != null) _openOriginDocumentbutton.Sensitive = selectedRow.StockMovimentIn.AttachedFile != null; else _openOriginDocumentbutton.Sensitive = false;

                    _treeViewXPO_ArticleHistory.Navigator.ButtonDelete.Sensitive = true;
                    _openCompositeArticlebutton.Sensitive = true;
                    _openSellDocumentbutton.Sensitive = selectedRow.StockMovimentOut != null;
                    _treeViewXPO_ArticleHistory.Navigator.ButtonUpdate.Sensitive = true;
                }
                else
                {
                    _treeViewXPO_ArticleHistory.Navigator.ButtonUpdate.Sensitive = false;
                    _openCompositeArticlebutton.Sensitive = false;
                    _printSerialNumberbutton.Sensitive = false;
                    _openChangeArticleLocationbutton.Sensitive = false;
                }
                if(selectedRow.StockMovimentOut != null)
                {
                    //_treeViewXPO_ArticleHistory.Navigator.ButtonUpdate.Sensitive = false;
                    _openChangeArticleLocationbutton.Sensitive = false;
                }

                //CheckBox check
                int columnIndexCheckBox = 1;

                try
                {
                    bool itemChecked = Convert.ToBoolean(_treeViewXPO_ArticleHistory.ListStoreModel.GetValue((sender as TreeViewArticleSerialNumber).TreeIterModel, columnIndexCheckBox));                    

                    if (itemChecked)
                    {
                        if(!_listArticleserialnumbers.Contains(selectedRow)) _listArticleserialnumbers.Add(selectedRow);

                    }
                    else
                    {
                        if (_listArticleserialnumbers.Contains(selectedRow)) _listArticleserialnumbers.Remove(selectedRow);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

        }

        private void OpenCompositeArticlebutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selecteRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                List<fin_articleserialnumber> pSelectedCollection = new List<fin_articleserialnumber>();
                if (selecteRow != null && selecteRow.Article.IsComposed && selecteRow.ArticleComposition.Count > 0)
                {
                    foreach (var item in selecteRow.ArticleComposition)
                    {
                        pSelectedCollection.Add(item.ArticleSerialNumberChild);
                    }
                }
                selecteRow.SerialNumber = Utils.OpenNewSerialNumberCompositePopUpWindow(this, selecteRow, out pSelectedCollection, selecteRow.SerialNumber, pSelectedCollection);

                //Add modified items
                int i = 0;
                foreach (var item in selecteRow.ArticleComposition)
                {
                    if (pSelectedCollection[i] != null && pSelectedCollection[i].Oid != Guid.Empty)
                    {
                        item.ArticleSerialNumberChild = pSelectedCollection[i];
                    }
                    i++;
                }

                //Add new items if exists
                if (selecteRow.ArticleComposition.Count < pSelectedCollection.Count)
                {
                    for (int j = i; j < pSelectedCollection.Count; j++)
                    {
                        selecteRow.ArticleComposition.Add(new fin_articlecompositionserialnumber(selecteRow.Session) { ArticleSerialNumber = selecteRow, ArticleSerialNumberChild = pSelectedCollection[j] });
                    }
                }

                selecteRow.Save();
                _treeViewXPO_ArticleHistory.Refresh();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void OpenSellDocumentbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selecteRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                if (selecteRow.StockMovimentOut != null && selecteRow.StockMovimentOut.DocumentMaster != null)
                {
                    var fileToOpen = ProcessFinanceDocument.GenerateDocumentFinanceMasterPDFIfNotExists(selecteRow.StockMovimentOut.DocumentMaster);
                                       
                    if (File.Exists(fileToOpen))
                    {
                        if (Utils.IsLinux)
                        {
                            System.Diagnostics.Process.Start(FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, fileToOpen)));
                        }
                        else if (Utils.UsePosPDFViewer() == true && !Utils.IsLinux)
                        {
                            string docPath = FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, fileToOpen));
                            var ScreenSizePDF = GlobalApp.ScreenSize;
                            int widthPDF = ScreenSizePDF.Width;
                            int heightPDF = ScreenSizePDF.Height;
                            System.Windows.Forms.Application.Run(new startDocumentViewer(docPath, widthPDF - 50, heightPDF - 25, false));
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                //Utils.ShowMessageTouch(_sourceWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_error"), "***Cant Update Record ***");
            }
        }

        private void OpenOriginDocumentbutton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var selecteRow = _treeViewXPO_ArticleHistory.DataSourceRow as fin_articleserialnumber;
                if (selecteRow.StockMovimentIn != null && selecteRow.StockMovimentIn.AttachedFile != null)
                {
                    var fileToOpen = selecteRow.StockMovimentIn.AttachedFile;

                    System.IO.File.WriteAllBytes(selecteRow.StockMovimentIn.DocumentNumber + ".pdf", fileToOpen);                    
                    
                    if (File.Exists(selecteRow.StockMovimentIn.DocumentNumber + ".pdf"))
                    {
                        if (Utils.IsLinux)
                        {
                            System.Diagnostics.Process.Start(FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, selecteRow.StockMovimentIn.DocumentNumber + ".pdf")));
                        }
                        else if (Utils.UsePosPDFViewer() == true && !Utils.IsLinux)
                        {
                            string docPath = FrameworkUtils.OSSlash(string.Format(@"{0}\{1}", Environment.CurrentDirectory, selecteRow.StockMovimentIn.DocumentNumber + ".pdf"));
                            var ScreenSizePDF = GlobalApp.ScreenSize;
                            int widthPDF = ScreenSizePDF.Width;
                            int heightPDF = ScreenSizePDF.Height;
                            System.Windows.Forms.Application.Run(new startDocumentViewer(docPath, widthPDF - 50, heightPDF - 25, false));
                        }
                        File.Delete(selecteRow.StockMovimentIn.DocumentNumber + ".pdf");
                    }
                }

                ////Initialize modelValues from DataSourceRow
                //System.Object[] modelValues = _treeViewXPO_ArticleHistory.DataSourceRowToModelRow(new fin_articleserialnumber());

                ////_listStoreModel.SetValues(_treeIter, modelValues);
                //_treeViewXPO_ArticleHistory.ListStoreModel.SetValues(_treeViewXPO_ArticleHistory.TreeIterModel, modelValues);

                //Required to Store _treePath to SetCursor after UpdateModelsAfterChanges()
                //_treeViewXPO_ArticleHistory.TreeView.GetCursor(out _treeViewXPO_ArticleHistory._treePath, out _treeViewXPO_ArticleHistory._treeViewColumn);

                ////Update ModelFilter after changes in Base Model
                ////UpdateChildModelsAfterCRUDChanges();

                ////Cursor Work - Must be After Assign UpdateModelsAfterChanges()
                //_treeViewXPO_ArticleHistory.TreeView.SetCursor(_treeViewXPO_ArticleHistory._treePath, _treeViewXPO_ArticleHistory._treeViewColumn, false);

                //var teste = _treeViewXPO_ArticleHistory.DataSourceRowGetColumnValue(_treeViewXPO_ArticleHistory.DataSourceRow, _treeViewXPO_ArticleHistory._treePath.Indices[0], "SerialNumber");

                //_treeViewXPO_ArticleHistory.TreeView.ScrollToCell(_treeViewXPO_ArticleHistory.ListStoreModelFilterSort.GetPath(_treeViewXPO_ArticleHistory._treeIter), _treeViewXPO_ArticleHistory.TreeView.Columns[0], false, 0, 0);


                //Utils.ShowMessageTouch(_sourceWindow, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_information"), "***Record Updated***");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                //Utils.ShowMessageTouch(_sourceWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_error"), "***Cant Update Record ***");
            }
        }

        //FinanceMaster: Get Article Serial Number List (Checked Items)

        private bool TreeModelForEachTask_ActionGetFinanceDocumentsList(TreeModel model, TreePath path, TreeIter iter)
        {
            int columnIndexCheckBox = 1;
            int columnIndexGuid = 2;
            try
            {
                bool itemChecked = Convert.ToBoolean(model.GetValue(iter, columnIndexCheckBox));
                Guid itemGuid = new Guid(model.GetValue(iter, columnIndexGuid).ToString());

                if (itemChecked)
                {
                    //Add to FinanceMasterDocuments
                    _listArticleserialnumbers.Add((fin_articleserialnumber)FrameworkUtils.GetXPGuidObject(typeof(fin_articleserialnumber), itemGuid));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
            return false;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        public TouchButtonIconWithText GetNewButton(string pId, string pLabel, string pIcon)
        {
            TouchButtonIconWithText result = null;
            try
            {
                String fileIcon = FrameworkUtils.OSSlash(GlobalFramework.Path["images"] + pIcon);
                String fontBaseDialogActionAreaButton = FrameworkUtils.OSSlash(GlobalFramework.Settings["fontBaseDialogActionAreaButton"]);
                Color colorBaseDialogActionAreaButtonBackground = Color.Transparent;  
                Color colorBaseDialogActionAreaButtonFont = FrameworkUtils.StringToColor(GlobalFramework.Settings["colorBaseDialogActionAreaButtonFont"]);
                Size sizeBaseDialogActionAreaBackOfficeNavigatorButton = Utils.StringToSize(GlobalFramework.Settings["sizeBaseDialogActionAreaBackOfficeNavigatorButton"]);
                Size sizeBaseDialogActionAreaBackOfficeNavigatorButtonIcon = Utils.StringToSize(GlobalFramework.Settings["sizeBaseDialogActionAreaBackOfficeNavigatorButtonIcon"]);

                result = new TouchButtonIconWithText(pId, colorBaseDialogActionAreaButtonBackground, pLabel, fontBaseDialogActionAreaButton, colorBaseDialogActionAreaButtonFont, fileIcon, sizeBaseDialogActionAreaBackOfficeNavigatorButtonIcon, sizeBaseDialogActionAreaBackOfficeNavigatorButton.Width, sizeBaseDialogActionAreaBackOfficeNavigatorButton.Height);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
            return (result);
        }
    }
}

