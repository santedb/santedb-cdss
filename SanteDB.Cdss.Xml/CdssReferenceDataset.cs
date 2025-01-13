/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 */
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Data.Import;
using SanteDB.Core.Data.Import.Format;
using SanteDB.Core.Model.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Represents a CDSS dataset definition or a part of one with methods for filtering and selecting data
    /// </summary>
    public class CdssReferenceDataset : IEnumerable<IForeignDataRecord>
    {

        private readonly CdssReferenceDataset m_parentDataset;
        private readonly CdssDatasetDefinition m_datasetData;
        private readonly Func<IForeignDataRecord, bool> m_filterFunction;

        /// <summary>
        /// Create a new dataset from the specified source dataset
        /// </summary>
        /// <param name="sourceDataset">The dataset for the object</param>
        internal CdssReferenceDataset(CdssDatasetDefinition sourceDataset)
        {
            this.m_parentDataset = null;
            this.m_datasetData = sourceDataset;
        }

        /// <summary>
        /// Reference dataset created as a child of another (for filters)
        /// </summary>
        /// <param name="parentDataset">The dataset which is the parent of this dataset</param>
        /// <param name="filterFunction">The filtering function</param>
        private CdssReferenceDataset(CdssReferenceDataset parentDataset, Func<IForeignDataRecord, bool> filterFunction)
        {
            this.m_parentDataset = parentDataset;
            this.m_datasetData = null;
            this.m_filterFunction = filterFunction;
        }

        /// <summary>
        /// Lookup a value
        /// </summary>
        public CdssReferenceDataset Lookup(String columnName, object filterValue) => new CdssReferenceDataset(this, p => MapUtil.TryConvert(p[columnName], filterValue.GetType(), out var comparitor) && comparitor.Equals(filterValue));

        /// <summary>
        /// Lookup a value by a key between the ranages
        /// </summary>
        public CdssReferenceDataset Between(String columnName, object lowerValue, object upperValue) => new CdssReferenceDataset(this, p => MapUtil.TryConvert(p[columnName], lowerValue.GetType(), out var comparitor) && ((IComparable)comparitor).CompareTo(lowerValue) >= 0 && ((IComparable)comparitor).CompareTo(upperValue) <= 0);

        /// <summary>
        /// Select the value in the specified column
        /// </summary>
        public IEnumerable<Object> Select(String columnName)
        {
            foreach (var itm in this)
            {
                yield return itm[columnName];
            }
        }

        /// <summary>
        /// Select values of a particular type
        /// </summary>
        public IEnumerable<double> SelectReal(string columnName) => this.Select(columnName).OfType<double>();

        /// <summary>
        /// Select values of a particular type
        /// </summary>
        public IEnumerable<int> SelectInt(string columnName) => this.Select(columnName).OfType<int>();

        /// <summary>
        /// Select values of a particular type
        /// </summary>
        public IEnumerable<long> SelectLong(string columnName) => this.Select(columnName).OfType<long>();

        /// <summary>
        /// Select as a date time
        /// </summary>
        public IEnumerable<DateTime> SelectDate(string columnName) => this.Select(columnName).OfType<DateTime>();

        /// <inheritdoc/>
        public IEnumerator<IForeignDataRecord> GetEnumerator()
        {
            if (this.m_parentDataset != null)
            {
                foreach (var itm in this.m_parentDataset)
                {
                    if (this.m_filterFunction?.Invoke(itm) != false)
                    {
                        yield return itm;
                    }
                }
            }
            else if (this.m_datasetData != null)
            {
                using (var ms = new MemoryStream(this.m_datasetData.RawData))
                {
                    using (var fdr = new CsvForeignDataFormat().Open(ms).CreateReader())
                    {
                        while (fdr.MoveNext())
                        {
                            yield return fdr;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}