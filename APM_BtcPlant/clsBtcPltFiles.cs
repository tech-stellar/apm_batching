using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace APM_BtcPlant
{
    public class clsBtcPltFiles
    {
        private List<clsBtcPltFileAttr> objBtcPltFileAttrs = new List<clsBtcPltFileAttr>();

        public int intBtcPltFileCount
        {
            get
            { return objBtcPltFileAttrs.Count(); }

        }

        public List<clsBtcPltFileAttr> mobjBtcPltFileAttrs
        {
            get
            { return objBtcPltFileAttrs; }
        }

        public void scanBtcPltFolder(string strBtcPltFolderPath)
        {
            if (Directory.Exists(strBtcPltFolderPath ))
            {
                string[] fileEntries = Directory.GetFiles(strBtcPltFolderPath);

                foreach (string fileName in fileEntries)
                {
                    addBtcPltFile(Path.GetFileName(fileName), fileName);
                }
            }
        }

        public void moveBtcPltFile(clsBtcPltFileAttr oBtcPltFileAttr, string strFileMoveTo)
        {
            if(Directory.Exists(strFileMoveTo))
            {
                try
                {
                    File.Copy(oBtcPltFileAttr.BtcPltFilePath, strFileMoveTo + "\\" + oBtcPltFileAttr.BtcPltFileName, true);
                    File.Delete(oBtcPltFileAttr.BtcPltFilePath);
                }
                catch (Exception ex)
                {

                }
            }
            
        }

        private void addBtcPltFile(string strFileName, string strFilePath)
        {
            clsBtcPltFileAttr objBtcPltFileAttr = new clsBtcPltFileAttr();
            objBtcPltFileAttr.BtcPltFileName = strFileName;
            objBtcPltFileAttr.BtcPltFilePath = strFilePath;

            objBtcPltFileAttrs.Add(objBtcPltFileAttr);
        }

        public void readBtcPltFile( clsBtcPltFileAttr oBtcPltFileAttr)
        {

            string[] fileLines = File.ReadAllLines(oBtcPltFileAttr.BtcPltFilePath);

            if (fileLines.Count() == 2) 
            {
                string[] LineOne = fileLines[0].Split(',');
                string[] LineTwo = fileLines[1].Split(',');

                int intMaxCol = LineOne.Count() - 1;
                int intItemColPost = LineOne.Count() - 4;
                int intTimeColPost = LineOne.Count() - 3;
                int intBtcLineColPost = LineOne.Count() -2;
                // modify here for add in truck number
                int intTruckColPost = LineOne.Count() - 1;

                for (int i = 0; i <= intMaxCol; i++)
                {
                    switch (i)
                    {
                        case 0:
                            oBtcPltFileAttr.BtcPltDate = LineTwo[i].ToString();
                            break;
                        case 1:
                            oBtcPltFileAttr.BtcPltItemCode = LineTwo[i].ToString();
                            break;
                        case 2:
                            oBtcPltFileAttr.BtcPltPlantCode = LineTwo[i].ToString();
                            break;
                        case 3:
                            oBtcPltFileAttr.BtcPltDocketNum = LineTwo[i].ToString();
                            break;
                        case 4:
                            oBtcPltFileAttr.BtcPltItemQty = Convert.ToDecimal(LineTwo[i].ToString());
                            break;
                        default:
                            if (i == intItemColPost)
                            {
                                // do nothing about it 
                            }
                            else if (i == intTimeColPost)
                            {
                                oBtcPltFileAttr.BtcPltBatchTime = LineTwo[i].ToString();
                            }
                            else if (i == intBtcLineColPost)
                            {
                                oBtcPltFileAttr.BtcPltBatchLine = LineTwo[i].ToString();
                            }
                            // modify here for add in truck number
                            else if (i == intTruckColPost)
                            {
                                oBtcPltFileAttr.BtcPltTruckNo = LineTwo[i].ToString();
                            }
                            else
                            {
                                if (!LineOne[i].Equals("") && Convert.ToDecimal(LineTwo[i].ToString()) > 0)
                                {
                                    oBtcPltFileAttr.addBtcPltMat(LineOne[i].ToString(), Convert.ToDecimal(LineTwo[i].ToString()));
                                }

                            }



                            break;
                    }

                }

            }

            //return objBtcFileProp;
        }

        public bool checkIsFileContentEmpty(clsBtcPltFileAttr oBtcPltFileAttr)
        {
            bool isEmptyFile =false;

            if (new FileInfo(oBtcPltFileAttr.BtcPltFilePath).Length == 0)
            {
                // file is empty
                isEmptyFile = true;            
                    }
           else
            {
                // there is something in it
                isEmptyFile = false;
            }

            return isEmptyFile;
       }
        public Boolean checkCompany(clsAppConfigs oConfig)
        {

            try
            {
                Boolean boolChecked = true;

                string strSql = "select CheckBox10 from Company where Company = 'APM'";
                DataTable dtBtcPlt = new DataTable();

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                    sqlAdapter.SelectCommand = sqlCmd;
                    sqlAdapter.Fill(dtBtcPlt);

                    if (dtBtcPlt.Rows.Count != 0)
                    {
                        foreach (DataRow row in dtBtcPlt.Rows)
                        {
                            boolChecked = (Boolean)row["CheckBox10"];
                        }
                    }

                    sqlAdapter.Dispose();
                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                    return boolChecked;
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }
        }

        public void updateCompany(Boolean boolChecked, clsAppConfigs oConfig)
        {

            try
            {
                string strSql = "update Company set CheckBox10 = " + (boolChecked?"1":"0") + " where Company = 'APM'";

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();
                    sqlCmd.ExecuteNonQuery();
                    
                    sqlCmd.Dispose();
                    sqlConn.Dispose();
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();
            }
        }

        public Boolean mapBtcPltWithEpic(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {

            try
            {
                Boolean boolError;

                string strSql = String.Format("select top 1 ShortChar01,ShortChar02,ShortChar03,ShortChar04,ShortChar05 from ice.UD37 where key1 = '{0}'", oBtcPltFileAttr.BtcPltPlantCode);
                DataTable dtBtcPlt = new DataTable();

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                    sqlAdapter.SelectCommand = sqlCmd;
                    sqlAdapter.Fill(dtBtcPlt);

                    if (dtBtcPlt.Rows.Count != 0)
                    {
                        foreach (DataRow row in dtBtcPlt.Rows)
                        {
                            oBtcPltFileAttr.EpicCompany = row["ShortChar01"].ToString();
                            oBtcPltFileAttr.EpicWarehouse = row["ShortChar03"].ToString();
                            oBtcPltFileAttr.EpicPlant = row["ShortChar02"].ToString();
                            oBtcPltFileAttr.EpicBin = row["ShortChar04"].ToString();
                            oBtcPltFileAttr.EpicJobOpr = row["ShortChar05"].ToString();
                        }

                        boolError = false;
                    }

                    else
                    {
                        string strError = String.Format("Unable to find setup for mapping plant {0}.", oBtcPltFileAttr.BtcPltPlantCode);

                        clsEventLogger objELogger = new clsEventLogger();
                        objELogger.beginLogging(strError, EventLogEntryType.Error);
                        objELogger.Dispose();

                        clsError oError = new clsError();
                        oError.ErrDescription = strError;
                        oBtcPltFileAttr.addError(oError);

                        boolError = true;
                    }

                    sqlAdapter.Dispose();
                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                    return (boolError ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }
        }

        public Boolean mapBtcPltFGWithEpic(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    int intErrorCount = 0;

                    string strSql = String.Format("select top 1 shortchar01 from ice.UD36 where key1 = '{0}'", oBtcPltFileAttr.BtcPltItemCode);
                    DataTable dtBtcPltMat = new DataTable();

                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                    sqlAdapter.SelectCommand = sqlCmd;

                    sqlAdapter.Fill(dtBtcPltMat);

                    if (dtBtcPltMat.Rows.Count != 0)
                    {
                        foreach (DataRow row in dtBtcPltMat.Rows)
                        { oBtcPltFileAttr.EpicItemCode = row["shortchar01"].ToString(); }
                            
                    }
                    else
                    {
                        string strError = String.Format("Unable to find setup for mapping FG code {0}.", oBtcPltFileAttr.BtcPltItemCode);

                        clsEventLogger objELogger = new clsEventLogger();
                        objELogger.beginLogging(strError, EventLogEntryType.Error);
                        objELogger.Dispose();

                        clsError oError = new clsError();
                        oError.ErrDescription = strError;
                        oBtcPltFileAttr.addError(oError);

                        intErrorCount++;
                    }

                    sqlAdapter.Dispose();
                    sqlCmd.Connection.Close();
                    sqlCmd.Dispose();

                    return (intErrorCount > 0 ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }
        }

        public Boolean mapBtcPltMaterialWithEpic(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    int intErrorCount = 0;

                    foreach (clsBtcPltMat oBtcMat in oBtcPltFileAttr.mobjBtcPltMats)
                    {
                        
                        string strSql = String.Format("select top 1 shortchar01 from ice.UD36 where key1 = '{0}'", oBtcMat.strBtcPltMatCode);
                        DataTable dtBtcPltMat = new DataTable();

                        SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                        sqlCmd.Connection.Open();

                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.SelectCommand = sqlCmd;
                        sqlAdapter.Fill(dtBtcPltMat);

                        if (dtBtcPltMat.Rows.Count != 0)
                        {
                            foreach (DataRow row in dtBtcPltMat.Rows)
                            {
                                oBtcMat.strEpicorMatCode = row["shortchar01"].ToString();
                            }
                            
                        }
                        else
                        {
                            string strError = String.Format("Unable to find setup for mapping material code {0}.", oBtcMat.strBtcPltMatCode);

                            clsEventLogger objELogger = new clsEventLogger();
                            objELogger.beginLogging(strError, EventLogEntryType.Error);
                            objELogger.Dispose();

                            clsError oError = new clsError();
                            oError.ErrDescription = strError;
                            oBtcPltFileAttr.addError(oError);

                            intErrorCount ++;
                        }

                        sqlAdapter.Dispose();
                        sqlCmd.Connection.Close();
                        sqlCmd.Dispose();



                    }

                    sqlConn.Dispose();
                    return (intErrorCount > 0 ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }
        }

        public Boolean loadPartInfoForFG(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {
                Boolean boolError;
                string strSql = String.Format("select top 1 partdescription, ium from erp.Part where company = '{0}' and partnum = '{1}'", oBtcPltFileAttr.EpicCompany,oBtcPltFileAttr.EpicItemCode);
                DataTable dtPart = new DataTable();

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                    sqlAdapter.SelectCommand = sqlCmd;
                    sqlAdapter.Fill(dtPart);

                    if (dtPart.Rows.Count != 0)
                    {
                        foreach (DataRow row in dtPart.Rows)
                        {
                            oBtcPltFileAttr.EpicItemDesc = row["partdescription"].ToString();
                            oBtcPltFileAttr.EpicItemUOM = row["ium"].ToString();
                        }

                        boolError = false;
                    }

                    else
                    {
                        string strError = String.Format("Unable to find details for item {0} in Epicor.", oBtcPltFileAttr.EpicItemCode);

                        clsEventLogger objELogger = new clsEventLogger();
                        objELogger.beginLogging(strError, EventLogEntryType.Error);
                        objELogger.Dispose();

                        clsError oError = new clsError();
                        oError.ErrDescription = strError;
                        oBtcPltFileAttr.addError(oError);

                        boolError = true;
                    }

                    sqlAdapter.Dispose();
                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                    return (boolError ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }

        }

        public Boolean loadPartInfoForMaterial(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    int intErrorCount = 0;

                    foreach (clsBtcPltMat oBtcMat in oBtcPltFileAttr.mobjBtcPltMats)
                    {

                        string strSql = String.Format("select top 1 partdescription, ium from erp.Part where company = '{0}' and partnum = '{1}'",oBtcPltFileAttr.EpicCompany, oBtcMat.strEpicorMatCode);
                        DataTable dtPart = new DataTable();

                        SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                        sqlCmd.Connection.Open();

                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.SelectCommand = sqlCmd;
                        sqlAdapter.Fill(dtPart);

                        if (dtPart.Rows.Count != 0)
                        {
                            foreach (DataRow row in dtPart.Rows)
                            {
                                oBtcMat.strEpicorMatDesc = row["partdescription"].ToString();
                                oBtcMat.strEpicorMatUOM = row["ium"].ToString();
                            }

                        }
                        else
                        {
                            string strError = String.Format("Unable to find part for code {0}.", oBtcMat.strEpicorMatCode);

                            clsEventLogger objELogger = new clsEventLogger();
                            objELogger.beginLogging(strError, EventLogEntryType.Error);
                            objELogger.Dispose();

                            clsError oError = new clsError();
                            oError.ErrDescription = strError;
                            oBtcPltFileAttr.addError(oError);

                            intErrorCount++;
                        }

                        sqlAdapter.Dispose();
                        sqlCmd.Connection.Close();
                        sqlCmd.Dispose();



                    }

                    sqlConn.Dispose();
                    return (intErrorCount > 0 ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }

        }

        public Boolean chkMaterialOnHandQty(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    int intErrorCount = 0;
                    decimal qtyOnHand = 0;

                    foreach (clsBtcPltMat oBtcMat in oBtcPltFileAttr.mobjBtcPltMats)
                    {

                        string strSql = String.Format("select isnull(sum(onhandqty),0) as qty from erp.PartBin where company = '{0}' and warehousecode='{1}' and binnum = '{2}' and partnum = '{3}'", oBtcPltFileAttr.EpicCompany, oBtcPltFileAttr.EpicWarehouse, oBtcPltFileAttr.EpicBin, oBtcMat.strEpicorMatCode);

                        SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                        sqlCmd.Connection.Open();

                        qtyOnHand = (decimal)sqlCmd.ExecuteScalar();

                        if (qtyOnHand < oBtcMat.strBtcPltMatUsage)
                        {
                            clsError oError = new clsError();
                            oError.ErrDescription = string.Format("Insufficient quantity on hand for material {0}", oBtcMat.strEpicorMatCode);
                            oBtcPltFileAttr.addError(oError);
                           
                            intErrorCount++;
                        }
                      
                        sqlCmd.Connection.Close();
                        sqlCmd.Dispose();

                    }

                    sqlConn.Dispose();
                    return (intErrorCount > 0 ? false : true);
                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

                return false;
            }

        }

        public void genJobNum(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {
                string strTempJobNum = "B" + DateTime.Now.ToString("yyyyMMdd");
                string strLastJobNum = "";
                string strSql = String.Format("select isnull(max(jobnum),'') from erp.jobhead where company = '{0}' and jobnum like '{1}%'", oBtcPltFileAttr.EpicCompany, strTempJobNum);

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    strLastJobNum = (string)sqlCmd.ExecuteScalar();

                    if (strLastJobNum.Equals(""))
                    { strTempJobNum = strTempJobNum + string.Format("{0:0000}", 1); }
                    else
                    { 
                        int iLastNum = int.Parse(  strLastJobNum.Substring(9));
                        strTempJobNum = strTempJobNum + string.Format("{0:0000}", iLastNum + 1);
                    }

                    oBtcPltFileAttr.EpicJobNum = strTempJobNum;

                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

            }

        }

        public Boolean verifyDuplicateBtcPltFile(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {
                int iRowCount = 0;
                string strSql = String.Format("select isnull(count(1),0) from ice.UD38 where Character04 = '{0}' and ShortChar02 ='Success' ", oBtcPltFileAttr.BtcPltFileName);

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    iRowCount = (int)sqlCmd.ExecuteScalar();

                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                }

                if (iRowCount == 0)
                { return true; }
                else
                {
                    clsError oError = new clsError();
                    oError.ErrDescription = string.Format("Error. File: {0} has been imported into the Epicor before.", oBtcPltFileAttr.BtcPltFileName);
                    oBtcPltFileAttr.addError(oError);

                    return false;
                }


            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();
                return false;
            }

        
        }

        public void genProcessID(clsBtcPltFileAttr oBtcPltFileAttr, clsAppConfigs oConfig)
        {
            try
            {
                string strTempProcessID = "P" + DateTime.Now.ToString("yyyyMMdd") + "-";
                string strLastProcessID = "";
                string strSql = String.Format("select isnull(max(key1),'') from ice.UD38 where key1 like '{0}%'",  strTempProcessID);

                using (SqlConnection sqlConn = new SqlConnection(oConfig.strEpicorDB))
                {
                    SqlCommand sqlCmd = new SqlCommand(strSql, sqlConn);
                    sqlCmd.Connection.Open();

                    strLastProcessID = (string)sqlCmd.ExecuteScalar();

                    if (strLastProcessID.Equals(""))
                    { strTempProcessID = strTempProcessID + string.Format("{0:00000}", 1); }
                    else
                    {
                        int iLastNum = int.Parse(strLastProcessID.Substring(10, 5).ToString());
                        strTempProcessID = strTempProcessID + string.Format("{0:00000}", iLastNum + 1);
                    }

                    oBtcPltFileAttr.ProcessID = strTempProcessID;

                    sqlCmd.Dispose();
                    sqlConn.Dispose();

                }

            }
            catch (Exception e)
            {
                clsEventLogger objELogger = new clsEventLogger();
                objELogger.beginLogging(e.Message, EventLogEntryType.Error);
                objELogger.Dispose();

            }
        
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
