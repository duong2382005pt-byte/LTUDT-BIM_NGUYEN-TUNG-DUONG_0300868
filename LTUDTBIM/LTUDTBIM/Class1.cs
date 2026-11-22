using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms; // Hoặc Autodesk.Revit.UI.TaskDialog nếu không dùng WinForms
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace C001_GetParameters
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            IList<Element> wallList = collector
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();

            StringBuilder sb = new StringBuilder();
            foreach (Element wall in wallList) // for each wall
            {
                sb.AppendLine(wall.Name + "::" + wall.Id);
                foreach (Parameter para in wall.Parameters) // for each parameter
                {
                    sb.Append(this.getParameterInfomation(para, doc, ":")); // print parameter information
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString(), "Wall Parameters");
            }
            else
            {
                MessageBox.Show("Không tìm thấy tường (Wall) nào trong dự án.", "Thông báo");
            }

            return Result.Succeeded;
        }

        // Giữ đúng tên getParameterInfomation như trong ảnh để khớp với lời gọi ở trên
        private string getParameterInfomation(Parameter para, Document doc, string separator)
        {
            if (para == null || para.Definition == null)
            {
                return string.Empty;
            }

            string paramName = para.Definition.Name;
            string paramValue = string.Empty;

            switch (para.StorageType)
            {
                case StorageType.Double:
                    // Có thể dùng para.AsValueString() để lấy giá trị kèm đơn vị hiển thị trên giao diện
                    paramValue = para.AsValueString() ?? para.AsDouble().ToString();
                    break;
                case StorageType.Integer:
                    if (para.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
                    {
                        paramValue = para.AsInteger() == 1 ? "Yes" : "No";
                    }
                    else
                    {
                        paramValue = para.AsValueString() ?? para.AsInteger().ToString();
                    }
                    break;
                case StorageType.String:
                    paramValue = para.AsString() ?? string.Empty;
                    break;
                case StorageType.ElementId:
                    ElementId id = para.AsElementId();
                    if (id != null && id != ElementId.InvalidElementId)
                    {
                        Element elem = doc.GetElement(id);
                        paramValue = elem != null ? $"{elem.Name} (Id: {id.IntegerValue})" : id.IntegerValue.ToString();
                    }
                    else
                    {
                        paramValue = "None";
                    }
                    break;
                default:
                    paramValue = para.AsValueString() ?? string.Empty;
                    break;
            }

            return $"{paramName} {separator} {paramValue}";
        }
    }
}