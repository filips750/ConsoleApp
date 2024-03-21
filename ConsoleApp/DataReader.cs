namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        private ImportedObject Deserialize(string[] values)
        {
            ImportedObject importedObject = new ImportedObject();
            importedObject.Type = values[0];
            importedObject.Name = values[1];
            importedObject.Schema = values[2];
            importedObject.ParentName = values[3];
            importedObject.ParentType = values[4];
            importedObject.DataType = values[5];
            importedObject.IsNullable = values[6];
            return importedObject;
        }

        private string ClearData(string value)
        {
            return value.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
        }

        private ImportedObject CorrectObject(ImportedObject importedObject)
        {
            if (importedObject.IsNullable == null)
                return null;
            importedObject.Type = ClearData(importedObject.Type).ToUpper();
            importedObject.Name = ClearData(importedObject.Name);
            importedObject.Schema = ClearData(importedObject.Schema);
            importedObject.ParentName = ClearData(importedObject.ParentName);
            importedObject.ParentType = ClearData(importedObject.ParentType).ToUpper();
            // if we want to compare the types of parent we need to use ToUpper there
            return importedObject;
        }

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            ImportedObjects = new List<ImportedObject>() { new ImportedObject() };
            var importedLines = new List<string>();
            try // add catching exceptions
            {
                using (var streamReader = new StreamReader(fileToImport)) // use 'using' keyword to properly free file handle 
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        importedLines.Add(line);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }

            // TODO decide if we can't join the above and below for loop so we won't pointlessly iterate 

            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                if (importedLine.Length == 0)
                {
                    continue;
                }
                var values = importedLine.Split(';');
                if (values.Length < 7)
                {
                    continue;
                    // TODO
                    // Decide if there is any need of handling errors if the data is corrupted
                }
                var importedObject = Deserialize(values);
                ((List<ImportedObject>)ImportedObjects).Add(importedObject);
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                importedObject = CorrectObject(importedObject); // clearing and correcting imported data moved here 
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type && impObj.ParentName == importedObject.Name)
                    {
                        importedObject.NumberOfChildren++;
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type != "DATABASE")
                {
                    continue; // Getting rid of indentation
                }

                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                // print all database's tables
                foreach (var table in ImportedObjects)
                {
                    if (table.IsNullable == null)
                    {
                        continue;
                    }
                    if (table.ParentType.ToUpper() == database.Type && table.ParentName == database.Name)
                    {
                        Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                        // print all table's columns
                        foreach (var column in ImportedObjects)
                        {
                            if (column.IsNullable == null)
                            {
                                continue;
                            }
                            if (column.ParentType.ToUpper() == table.Type && column.ParentName == table.Name)
                            {
                                Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                            }
                        }
                    }
                }
            }
            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        // pick one standard of getters and setters
        public string Schema;
        public string ParentName;
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
