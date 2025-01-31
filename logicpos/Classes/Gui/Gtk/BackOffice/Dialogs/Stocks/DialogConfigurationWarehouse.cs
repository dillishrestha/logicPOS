﻿using Gtk;
using logicpos.datalayer.DataLayer.Xpo;
using logicpos.App;
using logicpos.Classes.Gui.Gtk.WidgetsGeneric;
using logicpos.resources.Resources.Localization;
using logicpos.Classes.Gui.Gtk.Widgets.BackOffice;
using logicpos.Classes.Enums.Dialogs;
using logicpos.datalayer.DataLayer.Xpo.Documents;
using logicpos.Classes.Gui.Gtk.Widgets.Buttons;
using System.Drawing;
using System;
using System.Collections.Generic;
using logicpos.shared.Classes.Utils;

namespace logicpos.Classes.Gui.Gtk.BackOffice.Dialogs.Configuration
{
    class DialogConfigurationWarehouse : BOBaseDialog
    {
        private ICollection<Tuple<fin_warehouselocation, Entry, BOWidgetBox, TouchButtonIcon, TouchButtonIcon, GenericCRUDWidgetXPO, HBox>> _warehouseLocationCollection;
        private fin_warehouse _Warehouse;
        private fin_warehouselocation _Warehouselocation;
        private ScrolledWindow _scrolledWindow;
        private VBox vboxTab2;
        private string iconAddRecord = FrameworkUtils.OSSlash(string.Format("{0}{1}", GlobalFramework.Path["images"], @"Icons/icon_pos_nav_new.png"));
        private string iconClearRecord = FrameworkUtils.OSSlash(string.Format("{0}{1}", GlobalFramework.Path["images"], @"Icons/Windows/icon_window_delete_record.png"));

        public DialogConfigurationWarehouse(Window pSourceWindow, GenericTreeViewXPO pTreeView, DialogFlags pFlags, DialogMode pDialogMode, XPGuidObject pXPGuidObject)
            : base(pSourceWindow, pTreeView, pFlags, pDialogMode, pXPGuidObject)
        {
            this.Title = Utils.GetWindowTitle(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warehouse"));
            _warehouseLocationCollection = new List<Tuple<fin_warehouselocation, Entry, BOWidgetBox, TouchButtonIcon, TouchButtonIcon, GenericCRUDWidgetXPO, HBox>>();
            if (Utils.IsLinux) SetSizeRequest(500, 373);
            else SetSizeRequest(500, 450);
            InitUI();
            InitNotes();
            ShowAll();
        }

        private void InitUI()
        {
            try
            {
                string lastArticleCode = "0";
                try
                {
                    //IN009261 BackOffice - Inserir mais auto-completes nos forms
                    if (GlobalFramework.DatabaseType.ToString() == "MSSqlServer")
                    {
                        string lastArticleSql = string.Format("SELECT MAX(CAST(Code AS INT))FROM fin_warehouse");
                        lastArticleCode = GlobalFramework.SessionXpo.ExecuteScalar(lastArticleSql).ToString();
                    }
                    else if (GlobalFramework.DatabaseType.ToString() == "SQLite")
                    {
                        string lastArticleSql = string.Format("SELECT MAX(CAST(Code AS INT))FROM fin_warehouse");
                        lastArticleCode = GlobalFramework.SessionXpo.ExecuteScalar(lastArticleSql).ToString();
                    }
                    else if (GlobalFramework.DatabaseType.ToString() == "MySql")
                    {
                        string lastArticleSql = string.Format("SELECT MAX(CAST(code AS UNSIGNED)) as Cast FROM fin_warehouse;");
                        lastArticleCode = GlobalFramework.SessionXpo.ExecuteScalar(lastArticleSql).ToString();
                    }

                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
                if (_dataSourceRow == null) _Warehouse = new fin_warehouse();
                else _Warehouse = (_dataSourceRow as fin_warehouse);
                //Tab1
                VBox vboxTab1 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };
                //Ord
                Entry entryOrd = new Entry();
                BOWidgetBox boxOrd = new BOWidgetBox(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_order"), entryOrd);
                vboxTab1.PackStart(boxOrd, false, false, 0);
                _crudWidgetList.Add(new GenericCRUDWidgetXPO(boxOrd, _dataSourceRow, "Ord", SettingsApp.RegexIntegerGreaterThanZero, true));

                //Code
                Entry entryCode = new Entry();
                BOWidgetBox boxCode = new BOWidgetBox(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_code"), entryCode);
                vboxTab1.PackStart(boxCode, false, false, 0);
                _crudWidgetList.Add(new GenericCRUDWidgetXPO(boxCode, _dataSourceRow, "Code", SettingsApp.RegexIntegerGreaterThanZero, true));

                //Designation
                Entry entryDesignation = new Entry();
                BOWidgetBox boxDesignation = new BOWidgetBox(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_designation"), entryDesignation);
                vboxTab1.PackStart(boxDesignation, false, false, 0);
                _crudWidgetList.Add(new GenericCRUDWidgetXPO(boxDesignation, _dataSourceRow, "Designation", SettingsApp.RegexAlfaNumericExtended, true));

                //Default
                CheckButton checkButtonDefault = new CheckButton(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_default_warehouse"));
                vboxTab1.PackStart(checkButtonDefault, false, false, 0);
                _crudWidgetList.Add(new GenericCRUDWidgetXPO(checkButtonDefault, _dataSourceRow, "IsDefault"));

                //Disabled
                CheckButton checkButtonDisabled = new CheckButton(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_disabled"));
                if (_dialogMode == DialogMode.Insert) checkButtonDisabled.Active = SettingsApp.BOXPOObjectsStartDisabled;
                vboxTab1.PackStart(checkButtonDisabled, false, false, 0);
                _crudWidgetList.Add(new GenericCRUDWidgetXPO(checkButtonDisabled, _dataSourceRow, "Disabled"));

                //Append Tab
                _notebook.AppendPage(vboxTab1, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_main_detail")));

                //Tab1
                vboxTab2 = new VBox(false, _boxSpacing) { BorderWidth = (uint)_boxSpacing };

                _scrolledWindow = new ScrolledWindow();
                _scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                _scrolledWindow.ModifyBg(StateType.Normal, Utils.ColorToGdkColor(System.Drawing.Color.White));
                _scrolledWindow.ShadowType = ShadowType.None;

                if (_Warehouse != null && _Warehouse.WarehouseLocation.Count > 0)
                {
                    foreach (var location in _Warehouse.WarehouseLocation)
                    {
                        XPGuidObject getLocationFromWarehouse = FrameworkUtils.GetXPGuidObject(typeof(fin_warehouselocation), location.Oid);
                        PopulateWarehouseLocationEntrys(getLocationFromWarehouse);
                    }
                }
                else
                {
                    PopulateWarehouseLocationEntrys(null);
                }
                int lcode = 0;
                lcode = Convert.ToInt32(lastArticleCode.ToString()) + 10;
                if (lcode != 10 && entryCode.Text == "") { entryOrd.Text = lcode.ToString(); entryCode.Text = lcode.ToString(); }

                //Append Tab
                _notebook.AppendPage(_scrolledWindow, new Label(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_locations"))); 
            }
            catch (System.Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void PopulateWarehouseLocationEntrys(XPGuidObject pDataSourceRow)
        {
            try
            {
                //Dynamic SerialNumber
                if (pDataSourceRow == null)
                {
                    pDataSourceRow = new fin_warehouselocation(_dataSourceRow.Session);
                }
                if ((pDataSourceRow as fin_warehouselocation).Warehouse == null) (pDataSourceRow as fin_warehouselocation).Warehouse = _Warehouse;
                HBox hboxLocation = new HBox(false, _boxSpacing);

                //Localização
                Entry entryLocation = new Entry();
                BOWidgetBox boxLocation = new BOWidgetBox(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_ConfigurationDevice_PlaceTerminal"), entryLocation);
                GenericCRUDWidgetXPO genericCRUDWidgetXPO = new GenericCRUDWidgetXPO(boxLocation, pDataSourceRow, "Designation", SettingsApp.RegexAlfaNumeric, true);
                _crudWidgetList.Add(genericCRUDWidgetXPO);
                hboxLocation.PackStart(boxLocation);

                //Apagar
                TouchButtonIcon buttonClearLocation = new TouchButtonIcon("touchButtonIcon", Color.Transparent, iconClearRecord, new Size(15, 15), 20, 15);
                hboxLocation.PackEnd(buttonClearLocation, false, false, 1);

                //Adicionar
                TouchButtonIcon buttonAddLocation = new TouchButtonIcon("touchButtonIcon", Color.Transparent, iconAddRecord, new Size(15, 15), 20, 15);
                hboxLocation.PackEnd(buttonAddLocation, false, false, 1);

                vboxTab2.PackStart(hboxLocation, false, false, 0);
                _scrolledWindow.Add(vboxTab2);

                //Events
                buttonAddLocation.Clicked += delegate
                {
                    PopulateWarehouseLocationEntrys(null);
                };
                buttonClearLocation.Clicked += ButtonClearLocation_Clicked; ;
                vboxTab2.ShowAll();

                //Add to collection
                _warehouseLocationCollection.Add(new Tuple<fin_warehouselocation, Entry, BOWidgetBox, TouchButtonIcon, TouchButtonIcon, GenericCRUDWidgetXPO, HBox>(pDataSourceRow as fin_warehouselocation, entryLocation, boxLocation, buttonClearLocation, buttonAddLocation, genericCRUDWidgetXPO, hboxLocation));

            }
            catch (Exception ex)
            {
                _log.Error("Error populating Locations Entrys : " + ex.Message);
            }
        }

        private void ButtonClearLocation_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                ResponseType responseType = Utils.ShowMessageNonTouch(this, DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "dialog_message_delete_record"), string.Format(resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_warning"), GlobalFramework.ServerVersion));

                if (responseType == ResponseType.Yes)
                {
                    foreach (var location in _warehouseLocationCollection)
                    {
                        if (_warehouseLocationCollection.Count == 1)
                        {
                            location.Item2.Text = "";
                            return;
                        }
                        else if (location.Item4.Equals(sender as TouchButtonIcon))
                        {
                            var xpObject = location.Item1;
                            xpObject.Delete();
                            var xpEntry = location.Item2;
                            var xpBoxWidget = location.Item3;
                            xpBoxWidget.Hide();
                            var xpButtonClear = location.Item4;
                            xpButtonClear.Hide();
                            var xpButtonAdd = location.Item5;
                            xpButtonAdd.Hide();
                            vboxTab2.Remove(location.Item7);
                            _crudWidgetList.Remove(location.Item6);
                            _warehouseLocationCollection.Remove(location);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error clear warehouse location Entrys : " + ex.Message);
            }
        }
    }
}
