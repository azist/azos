/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Tests.Nub.IO.Archiving
{
	/// <summary>
	/// Contains constants used in fake log bubblegumization.
	/// </summary>
  public static class FakeLogConstants
  {
    public static readonly Atom FAKE_APP_GOV = Atom.Encode("gov");
    public static readonly Atom FAKE_APP_BIZ = Atom.Encode("bizapp");
    public static readonly Atom FAKE_APP_TZT = Atom.Encode("fake");

    public static readonly Atom FAKE_CHANNEL_OPLOG = Atom.Encode("oplog");


    public const string FAKE_LOG_XML_REQ = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<Message FakeDatatypesVersion = ""BT""

				 FakeTransportVersion=""BT""
         FakeTransactionDomain=""XXXX""
         FakeTransactionVersion= ""BT""
         FakeStructuresVersion=""BT""
         FakeECLVersion=""BT""
				 xsi:noNamespaceSchemaLocation=""Faketransport.xsd""
         FakeVersion=""BT""
				 xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<FakeHeader>
		<To>
			<Primary>
				<Identification>000003</Identification>
				<Qualifier>IIN</Qualifier>
			</Primary>
			<Secondary>
				<Identification>X4Q2</Identification>
				<Qualifier>PCN</Qualifier>
			</Secondary>
		</To>
		<From>
			<Primary>
				<Identification>1234567111</Identification>
				<Qualifier>D</Qualifier>
			</Primary>
		</From>
		<MessageID>8045D45130CD41139FCC36781DA6FEFA</MessageID>
		<RelatesToMessageID></RelatesToMessageID>
		<SentTime>2020-01-07T09:30:00Z</SentTime>
		<SoftwareSenderCertificationID>2233445566</SoftwareSenderCertificationID>
	</FakeHeader>
	<FakeBody>
		<FakeRequest>
			<Patient>
				<Name>
					<LastName>DOE</LastName>
					<FirstName>JOHN</FirstName>
				</Name>
				<Gender>M</Gender>
				<DateOfBirth>1918-11-11</DateOfBirth>
			</Patient>
			<BenefitsCoordination>
				<PBMMemberID>JREREM03TEST2018</PBMMemberID>
				<CardholderID>JREREM03TEST2018</CardholderID>
				<GroupID>JREREM03</GroupID>
				<PersonCode>01</PersonCode>
			</BenefitsCoordination>
			<Product>
				<DrugCoded>
					<NDC>00310075590</NDC>
				</DrugCoded>
				<Quantity>
					<Value>30</Value>
					<CodeListQualifier>38</CodeListQualifier>
					<QuantityUnitOfMeasure>
						<Code>C48542</Code>
					</QuantityUnitOfMeasure>
				</Quantity>
				<DaysSupply>30</DaysSupply>
				<DispensedAsWrittenProductSelectionCode>0</DispensedAsWrittenProductSelectionCode>
			</Product>
			<Prescriber>
				<Identification>
					<NPI>1112223333</NPI>
				</Identification>
				<LastName>BODD</LastName>
			</Prescriber>
			<Pharmacy>
				<Identification>
					<NPI>1234567897</NPI>
				</Identification>
				<BusinessName>ELMO STREET PHARMACY</BusinessName>
				<PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber>
			</Pharmacy>
		</FakeRequest>
	</FakeBody>
</Message>
";

		public const string FAKE_LOG_FLOW_1 =
			@"{""cadr"":""15.1.33.44:53322"",""cagn"":""insomnia/2020.4.1"",""cprt"":""POST  http://APPSRV-0001:80/fake/check"",""mid"":""3045D45130CD41139FCC36781DA6FEFA"",""rel"":""<na>""}";

		public const string FAKE_LOG_FLOW_2 =
			@"{""cadr"":""15.1.33.55:53323"",""cagn"":""insomnia/2020.4.1"",""cprt"":""POST  http://APPSRV-0002:80/fake/check"",""mid"":""3045D45130CD41139FCC36781DA6FEFA"",""rel"":""<na>""}";

		public const string FAKE_LOG_FLOW_3 =
			@"{""cadr"":""15.1.33.66:53323"",""cagn"":""insomnia/2020.4.1"",""cprt"":""POST  http://WEBSRV-0002:80/fake/check"",""mid"":""3045D45130CD41139FCC36781DA6FEFA"",""rel"":""<na>""}";

		public const string FAKE_LOG_FLOW_4 =
			@"{""cadr"":""15.1.33.77:53323"",""cagn"":""insomnia/2020.4.1"",""cprt"":""POST  http://WEBSRV-0001:80/fake/check"",""mid"":""3045D45130CD41139FCC36781DA6FEFA"",""rel"":""<na>""}";

		public const string FAKE_LOG_JSON_REQ =
			@"{""channel"":""--any--"",""request"":{""Header"":{""To"":{""Primary"":{""Identification"":""000003"",""Qualifier"":""IIN""},""Secondary"":{""Identification"":""ABCD"",""Qualifier"":""PCN""}},""From"":{""Primary"":{""Identification"":""1234567111"",""Qualifier"":""D""}},""MessageID"":""8045D45130CD41139FCC36781DA6FEFA"",""SentTime"":""2020-01-07T09:30:00Z"",""SoftwareSenderCertificationID"":""2233445566""},""Body"":{""Item"":{""__entity"":""request"",""Patient"":{""DateOfBirth"":""1944-06-06T00:00:00Z"",""Gender"":""M"",""Name"":{""LastName"":""JONES"",""FirstName"":""GARY""}},""BenefitsCoordination"":{""PBMMemberID"":""NWIRON03TEST2018"",""CardholderID"":""JREREM03TEST2018"",""GroupID"":""JREREM03"",""PersonCode"":""01""},""ProductRequested"":{""DrugCoded"":{""NDC"":""00310075590""},""DaysSupply"":30,""DAWCode"":""0"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}}},""Prescriber"":{""Identification"":{""NPI"":""1112223338""},""LastName"":""HOSENFEFFER""},""Pharmacy"":{""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""}}},""DatatypesVersion"":""BT"",""TransportVersion"":""BT"",""TransactionDomain"":""XXXX"",""TransactionVersion"":""BT"",""StructuresVersion"":""BT"",""ECLVersion"":""BT"",""RTPBVersion"":""BT""}}";

		public const string FAKE_LOG_JSON_RESP =
			@"{""response"":{""Header"":{""To"":{""Primary"":{""Identification"":""--any--"",""Qualifier"":""D""}},""From"":{""Primary"":{""Identification"":""JREREMY"",""Qualifier"":""PY""},""Secondary"":{""Identification"":""009999"",""Qualifier"":""IIN""},""Tertiary"":{""Identification"":""ABCD"",""Qualifier"":""PCN""}},""MessageID"":""23aa768a1b7c433891cd9b69b43ca752"",""RelatesToMessageID"":""8045D45130CD41139FCC36781DA6FEFA"",""SentTime"":""2020-11-06T14:24:08.773Z"",""SoftwareSenderCertificationID"":""Jreremy""},""Body"":{""Item"":{""__entity"":""response"",""Response"":{""Item"":{""__entity"":""processed"",""Note"":""Processed"",""HelpDeskSupportType"":""3"",""HelpDeskBusinessUnits"":[{""HelpdeskBusinessUnitType"":""5"",""HelpDeskCommunicationNumbers"":{""Telephone"":{""Number"":""8003614542""}}}]}},""ResponseProduct"":{""DrugCoded"":{""NDC"":""00310075590""},""DrugDescription"":""N\/A"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]},""ResponseProductAlternatives"":[{""DrugCoded"":{""NDC"":""70515062930""},""DrugDescription"":""ALTOPREV     TAB 40MG ER"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]},{""DrugCoded"":{""NDC"":""66869020490""},""DrugDescription"":""LIVALO       TAB 2MG"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]},{""DrugCoded"":{""NDC"":""25208020109""},""DrugDescription"":""ZYPITAMAG    TAB 2MG"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]},{""DrugCoded"":{""NDC"":""13668017930""},""DrugDescription"":""ROSUVASTATIN TAB 5MG"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]},{""DrugCoded"":{""NDC"":""00093505698""},""DrugDescription"":""ATORVASTATIN TAB 10MG"",""ePAEnabled"":false,""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""PharmacyResponses"":[{""Pharmacy"":{""PharmacyType"":""A"",""Identification"":{""NPI"":""1234567897""},""BusinessName"":""ELMO STREET PHARMACY"",""PrimaryTelephoneNumber"":""3012225555""},""PricingAndCoverage"":{""PricingAndCoverageIndicator"":""1"",""CoverageStatusCode"":""1"",""Quantity"":{""Value"":30,""CodeListQualifier"":""38"",""QuantityUnitOfMeasure"":{""Code"":""C48542""}},""DaysSupply"":30,""EstimatedPatientFinancialResponsibility"":4,""PatientPayComponents"":[{""PatientPayComponentQualifier"":""05"",""PatientPayComponentAmount"":4}],""CoverageRestrictionCode"":[{""CoverageRestrictionCode"":""92""}]}}]}]}},""DatatypesVersion"":""BT"",""TransportVersion"":""BT"",""TransactionDomain"":""XXXX"",""TransactionVersion"":""BT"",""StructuresVersion"":""BT"",""ECLVersion"":""BT"",""RTPBVersion"":""BT""}}";

		public const string FAKE_LOG_XML_RESP =
			@"<?xml version=""1.0"" encoding=""UTF-8""?><RTPBMessage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" RTPBDatatypesVersion=""BT"" RTPBECLVersion=""BT"" RTPBStructuresVersion=""BT"" RTPBTransactionDomain=""XXXX"" RTPBTransactionVersion=""BT"" RTPBTransportVersion=""BT"" RTPBVersion=""BT""><RTPBHeader><To><Primary><Identification>--any--</Identification><Qualifier>D</Qualifier></Primary></To><From><Primary><Identification>JREREMY</Identification><Qualifier>PY</Qualifier></Primary><Secondary><Identification>009999</Identification><Qualifier>IIN</Qualifier></Secondary><Tertiary><Identification>ABCD</Identification><Qualifier>PCN</Qualifier></Tertiary></From><MessageID>23aa768a1b7c433891cd9b69b43ca752</MessageID><RelatesToMessageID>8045D45130CD41139FCC36781DA6FEFA</RelatesToMessageID><SentTime>2020-11-06T14:24:08.7737650Z</SentTime><SoftwareSenderCertificationID>Jreremy</SoftwareSenderCertificationID></RTPBHeader><RTPBBody><RTPBResponse><Response><Processed><Note>Processed</Note><HelpDeskSupportType>3</HelpDeskSupportType><HelpDeskBusinessUnit><HelpdeskBusinessUnitType>5</HelpdeskBusinessUnitType><HelpDeskCommunicationNumbers><Telephone><Number>8003614542</Number></Telephone></HelpDeskCommunicationNumbers></HelpDeskBusinessUnit></Processed></Response><ResponseAlternativeProduct><DrugCoded><NDC>70515062930</NDC></DrugCoded><DrugDescription>ALTOPREV     TAB 40MG ER</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELM STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseAlternativeProduct><ResponseAlternativeProduct><DrugCoded><NDC>66869020490</NDC></DrugCoded><DrugDescription>LIVALO       TAB 2MG</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELM STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseAlternativeProduct><ResponseAlternativeProduct><DrugCoded><NDC>25208020109</NDC></DrugCoded><DrugDescription>ZYPITAMAG    TAB 2MG</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELM STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseAlternativeProduct><ResponseAlternativeProduct><DrugCoded><NDC>13668017930</NDC></DrugCoded><DrugDescription>ROSUVASTATIN TAB 5MG</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELM STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseAlternativeProduct><ResponseAlternativeProduct><DrugCoded><NDC>00093505698</NDC></DrugCoded><DrugDescription>ATORVASTATIN TAB 10MG</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELM STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseAlternativeProduct><ResponseProduct><DrugCoded><NDC>00310075590</NDC></DrugCoded><DrugDescription>N/A</DrugDescription><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><ePAEnabled>false</ePAEnabled><PricingAndCoverages><Pharmacy><PharmacyType>A</PharmacyType><Identification><NPI>1234567897</NPI></Identification><BusinessName>ELMO STREET PHARMACY</BusinessName><PrimaryTelephoneNumber>3012225555</PrimaryTelephoneNumber></Pharmacy><PricingAndCoverage><PricingAndCoverageIndicator>1</PricingAndCoverageIndicator><CoverageStatusCode>1</CoverageStatusCode><Quantity><Value>30</Value><QuantityUnitOfMeasure><Code>C48542</Code></QuantityUnitOfMeasure><CodeListQualifier>38</CodeListQualifier></Quantity><DaysSupply>30</DaysSupply><EstimatedPatientFinancialResponsibility>4</EstimatedPatientFinancialResponsibility><PatientPayComponent><PatientPayComponentQualifier>05</PatientPayComponentQualifier><PatientPayComponentAmount>4</PatientPayComponentAmount></PatientPayComponent><CoverageRestriction><CoverageRestrictionCode>92</CoverageRestrictionCode></CoverageRestriction></PricingAndCoverage></PricingAndCoverages></ResponseProduct></RTPBResponse></RTPBBody></RTPBMessage>";

		public const string FAKE_LOG_DZERO_REQ =
			@"{""claim"":{""__entity"":""RequestMessage"",""Bin"":""009999"",""Version"":""D0"",""TransactionCount"":1,""ProcessorControlNumber"":""ABCD"",""TransactionCode"":""B1"",""ServiceProviderIdQualifier"":""01"",""ServiceProviderId"":""1234567897"",""DateOfService"":""2020-11-06T08:24:05.044-05:00"",""CertificationId"":""2233445566"",""PatientHeader"":{""__entity"":""TransactionRequestPatientHeader"",""Patient"":{""__entity"":""PatientSegment"",""PatientId"":""JREREMTEST2018"",""DateOfBirth"":""1999-06-06T00:00:00Z"",""PatientGenderCode"":""M"",""PatientFirstName"":""TEST"",""PatientLastName"":""REREMY""},""Insurance"":{""__entity"":""InsuranceSegment"",""CardholderId"":""JREREMTEST2018"",""GroupId"":""JREREM03"",""PersonCode"":""01"",""PatientRelationshipCode"":""01""}},""Requests"":[{""__entity"":""TransactionRequest"",""Claim"":{""__entity"":""ClaimSegment"",""PrescriptionServiceReferenceNumberQualifier"":""1"",""PrescriptionServiceReferenceNumber"":""1552220161"",""ProductServiceIdQualifier"":""03"",""ProductServiceId"":""00310075590"",""QuantityDispensed"":30,""DaysSupply"":30,""CompoundCode"":""1"",""DAWProductSelectionCode"":""0"",""DatePrescriptionWritten"":""2020-11-06T08:24:05.044-05:00"",""QuantityPrescribed"":30},""Clinical"":{""__entity"":""ClinicalSegment""},""PharmacyProvider"":{""__entity"":""PharmacyProviderSegment"",""ProviderIdQualifier"":""5"",""ProviderId"":""1234567897""},""Prescriber"":{""__entity"":""PrescriberSegment"",""PrescriberIdQualifier"":""01"",""PrescriberId"":""1112223338"",""PrescriberLastName"":""ELMO""},""Pricing"":{""__entity"":""PricingSegment"",""IngredientCostSubmitted"":99999.99,""UsualCustomaryCharge"":99999.99}}]}}";

		public const string FAKE_LOG_DZERO_RSP =
			@"{""got"":{""__entity"":""ResponseMessage"",""Version"":""D0"",""TransactionCode"":""B1"",""TransactionCount"":1,""HeaderResponseStatus"":""A"",""ServiceProviderIdQualifier"":""01"",""ServiceProviderId"":""1234567897"",""DateOfService"":""2020-11-06T00:00:00Z"",""PatientHeader"":{""__entity"":""TransactionResponsePatientHeader"",""Insurance"":{""__entity"":""InsuranceResponseSegment"",""GroupId"":""JREREM03"",""PlanId"":""FAK clon"",""PayerHealthPlanIdQualifier"":""03"",""PayerHealthPlanId"":""009999"",""CardholderId"":""JREREMTEST2018""},""Patient"":{""__entity"":""PatientResponseSegment"",""PatientFirstName"":""JON"",""PatientLastName"":""REREMY"",""DateOfBirth"":""1999-09-09T00:00:00-04:00""}},""Responses"":[{""__entity"":""TransactionResponse"",""Claim"":{""__entity"":""ClaimResponseSegment"",""PrescriptionServiceReferenceNumberQualifier"":""1"",""PrescriptionServiceReferenceNumber"":""001552220161""},""Pricing"":{""__entity"":""PricingResponseSegment"",""TotalAmountPaid"":291.83,""PatientPayAmount"":4,""IngredientCostPaid"":293.83,""DispensingFeePaid"":2,""PercentageTaxAmountPaid"":0,""IncentiveAmountPaid"":0,""OtherResponsePaidAmounts"":[],""BasisOfReimbursementDetermination"":""05"",""AmountAttributedToSalesTax"":0,""RemainingBenefitAmount"":0,""EstimatedGenericSavings"":292.94,""AmountAppliedToPeriodicDeductible"":0,""AmountOfCopay"":4,""AmountExceedingPeriodicBenefitMax"":0,""AmountAttributedToProcessorFee"":0,""AmountOfCoinsurance"":0,""AmountAttributedToBrandDrug"":0},""Status"":{""__entity"":""StatusResponseSegment"",""TransactionResponseStatus"":""P"",""AuthorizationNumber"":""818295001106TB"",""HelpDeskPhoneNumberQualifier"":""03"",""HelpDeskPhoneNumber"":""800-361-4542""}}]}}";

    public const string FAKE_LOG_HOST_1 = "APPSRV-0001";

    public const string FAKE_LOG_HOST_2 = "APPSRV-0002";

    public const string FAKE_LOG_HOST_3 = "WEBSRV-0001";

    public const string FAKE_LOG_HOST_4 = "APPSRV-0003";

    public const string FAKE_LOG_DIM_1 = @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.1.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFA"", ""pcn"" : ""ABCD"" }";

    public const string FAKE_LOG_DIM_2 = @"{ ""bin"" : ""009999"", ""chn"" : ""FAKE"", ""clr"" : ""15.2.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/02"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFB"", ""pcn"" : ""ABCD"" }";

    public const string FAKE_LOG_DIM_3 = @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.3.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/03"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFC"", ""pcn"" : ""ABCD"" }";

    public const string FAKE_LOG_DIM_4 = @"{ ""bin"" : ""009993"", ""chn"" : ""none"", ""clr"" : ""15.4.35.68:53399"", ""dctx"" : ""none"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFD"", ""pcn"" : ""DCBA"" }";

    public const string FAKE_LOG_DIM_5 = @"{ ""bin"" : ""009994"", ""chn"" : ""SUM"", ""clr"" : ""20.5.35.68:53399"", ""dctx"" : ""sum"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFE"", ""pcn"" : ""XYZ"" }";

    public const string FAKE_LOG_DIM_6 = @"{ ""bin"" : ""009994"", ""chn"" : ""RED"", ""clr"" : ""30.0.0.69:53399"", ""dctx"" : ""red"", ""mbr"" : ""mbr@pbm::REDTEST2018/16"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFF"", ""pcn"" : ""XYZ"" }";
  }
}
