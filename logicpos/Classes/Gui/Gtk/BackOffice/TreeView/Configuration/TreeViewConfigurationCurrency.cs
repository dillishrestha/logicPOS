﻿using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using Gtk;
using logicpos.App;
using logicpos.datalayer.DataLayer.Xpo;
using logicpos.Classes.Gui.Gtk.WidgetsGeneric;
using logicpos.resources.Resources.Localization;
using System;
using System.Collections.Generic;
using logicpos.Classes.Enums.GenericTreeView;

namespace logicpos.Classes.Gui.Gtk.BackOffice
{
    class TreeViewConfigurationCurrency : GenericTreeViewXPO
    {
        //Public Parametless Constructor Required by Generics
        public TreeViewConfigurationCurrency() { }

        public TreeViewConfigurationCurrency(Window pSourceWindow)
            : this(pSourceWindow, null, null, null) { }

        //XpoMode
        public TreeViewConfigurationCurrency(Window pSourceWindow, XPGuidObject pDefaultValue, CriteriaOperator pXpoCriteria, Type pDialogType, GenericTreeViewMode pGenericTreeViewMode = GenericTreeViewMode.Default, GenericTreeViewNavigatorMode pGenericTreeViewNavigatorMode = GenericTreeViewNavigatorMode.Default)
        {
            //Init Vars
            Type xpoGuidObjectType = typeof(cfg_configurationcurrency);
            //Override Default Value with Parameter Default Value, this way we can have diferent Default Values for GenericTreeView
            cfg_configurationcurrency defaultValue = (pDefaultValue != null) ? pDefaultValue as cfg_configurationcurrency : null;
            //Override Default DialogType with Parameter Dialog Type, this way we can have diferent DialogTypes for GenericTreeView
            Type typeDialogClass = (pDialogType != null) ? pDialogType : typeof(DialogConfigurationCurrency);

            //Config
            int fontGenericTreeViewColumn = Convert.ToInt16(GlobalFramework.Settings["fontGenericTreeViewColumn"]);

            //Configure columnProperties
            List<GenericTreeViewColumnProperty> columnProperties = new List<GenericTreeViewColumnProperty>();
            columnProperties.Add(new GenericTreeViewColumnProperty("Code") { Title = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_code"), MinWidth = 100 });
            columnProperties.Add(new GenericTreeViewColumnProperty("Designation") { Title = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_designation"), Expand = true });
            columnProperties.Add(new GenericTreeViewColumnProperty("Acronym") { Title = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_ConfigurationCurrency_Acronym") });
            //columnProperties.Add(new GenericTreeViewColumnProperty("ExchangeRate") { 
            //    Title = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_exchangerate, 
            //    Type = typeof(Decimal), 
            //    Alignment = 1.0F
            //    //WIP
            //    //FontDesc = new Pango.FontDescription() { Size = 11 }, // Removed, this will make Trees that use it has diferent row Sizes
            //    //,CellRenderer = new CellRendererText() { Alignment = Pango.Alignment.Right, Xalign = 1.0F }
            //});
            columnProperties.Add(new GenericTreeViewColumnProperty("UpdatedAt") { Title = resources.CustomResources.GetCustomResources(GlobalFramework.Settings["customCultureResourceDefinition"], "global_record_date_updated"), MinWidth = 150, MaxWidth = 150 });

            //Configure Criteria/XPCollection/Model
            //CriteriaOperator.Parse("Code >= 100 and Code <= 9999");
            CriteriaOperator criteria = pXpoCriteria;
            if (pXpoCriteria != null)
            {
                criteria = CriteriaOperator.Parse($"({pXpoCriteria.ToString()}) AND (DeletedAt IS NULL)");
            }
            else
            {
                criteria = CriteriaOperator.Parse($"(DeletedAt IS NULL)");
            }
            XPCollection xpoCollection = new XPCollection(GlobalFramework.SessionXpo, xpoGuidObjectType, criteria);

            //Call Base Initializer
            base.InitObject(
              pSourceWindow,                  //Pass parameter 
              defaultValue,                   //Pass parameter
              pGenericTreeViewMode,           //Pass parameter
              pGenericTreeViewNavigatorMode,  //Pass parameter
              columnProperties,               //Created Here
              xpoCollection,                  //Created Here
              typeDialogClass                 //Created Here
            );
        }
    }
}
