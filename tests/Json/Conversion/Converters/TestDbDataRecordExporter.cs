#region Copyright (c) 2005 Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
#endregion

namespace Jayrock.Json.Conversion.Converters
{
    #region Imports

    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestDbDataRecordExporter
    {
        [ Test ]
        public void EmptyTable()
        {
            Assert.AreEqual("[]", Format(new DataTable()));
        }

        [ Test ]
        public void OneFieldOneRow()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            AppendRow(table, 1);
            Assert.AreEqual("[{\"id\":1}]", Format(table));
        }

        [ Test ]
        public void TwoFieldsTwoRows()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));
            AppendRow(table, 1, "john");
            AppendRow(table, 2, "zack");
            Assert.AreEqual("[{\"id\":1,\"name\":\"john\"},{\"id\":2,\"name\":\"zack\"}]", Format(table));
        }

        [ Test ]
        public void NullFieldValues()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            AppendRow(table, 1);
            AppendRow(table, DBNull.Value);
            AppendRow(table, 3);
            AppendRow(table, new object[] { null });
            Assert.AreEqual("[{\"id\":1},{\"id\":null},{\"id\":3},{\"id\":null}]", Format(table));
        }

        static void AppendRow(DataTable table, params object[] values)
        {
            table.Rows.Add(values);
        }

        static string Format(DataTable table)
        {
            var writer = new JsonTextWriter();
            JsonConvert.Export(new FakeDataTableReader(table), writer);
            return writer.ToString();
        }

        //
        // NOTE: The .NET Framework 2.0 has a DataTableReader implementation.
        // The following is a stub implementation for testing under 1.x.
        //

        sealed class FakeDataTableReader : IDataReader, IEnumerable
        {
            readonly DataTable _table;
            int _index = -1;

            public FakeDataTableReader(DataTable table)
            {
                Debug.Assert(table != null);

                _table = table;
            }

            DataRow CurrentRow => _table.Rows[_index];

            public IEnumerator GetEnumerator()
            {
                return new DbEnumerator(this, false);
            }

            public void Close()
            {
                IsClosed = true;
            }

            public bool Read()
            {
                if (_index + 1 >= _table.Rows.Count)
                    return false;

                _index++;
                return true;
            }

            public int Depth => 0;

            public bool IsClosed { get; private set; }

            public int RecordsAffected => 0;

            public void Dispose()
            {
                Close();
            }

            public string GetName(int i)
            {
                return _table.Columns[i].ColumnName;
            }

            public string GetDataTypeName(int i)
            {
                return GetFieldType(i).Name;
            }

            public Type GetFieldType(int i)
            {
                return _table.Columns[i].DataType;
            }

            public int GetValues(object[] values)
            {
                var data = CurrentRow.ItemArray;
                var length = Math.Min(values.Length, data.Length);
                Array.Copy(data, 0, values, 0, length);
                return length;
            }

            public int FieldCount => _table.Columns.Count;

            public bool NextResult()
            {
                return false;
            }

            #region Unimplemented and uninteresting IDataRecord/Reader members

            //
            // The remaining IDataRecord/Reader members are not implemented
            // because DbEnumerator never seems to call them.
            //
            // NOTE: It may be that this assumption no longer holds true in
            // a future implementation of the DbEnumerator and there for
            // this test fixutre may entirely fail. Consider finishing the
            // implementation minimally.
            //

            public DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            public object GetValue(int i)
            {
                throw new NotImplementedException();
            }

            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public object this[int i] => throw new NotImplementedException();

            public object this[string name] => throw new NotImplementedException();

            #endregion
        }
    }
}
