export CLASSPATH=lib/antlr-3.3-complete.jar:$CLASSPATH

java org.antlr.Tool  ../'Script Types'/ModScript/*.g ../'Script Types'/XmlScript/CPL/CPLLexer.g ../'Script Types'/XmlScript/CPL/CPLParser.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/Fallout3.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/Fallout3.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/Fallout3.XmlScript/CPL/*.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/Fallout4.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/Fallout4.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/Fallout4.XmlScript/CPL/*.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/Skyrim.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/Skyrim.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/Skyrim.XmlScript/CPL/*.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/SkyrimGOG.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/SkyrimGOG.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/SkyrimGOG.XmlScript/CPL/*.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/SkyrimSE.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/SkyrimSE.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/SkyrimSE.XmlScript/CPL/*.g

cp ../'Script Types'/XmlScript/CPL/CPLImportLexer.g ../'Game Modes'/Starfield.XmlScript/CPL/CPLImportLexer.g
cp ../'Script Types'/XmlScript/CPL/CPLImportParser.g ../'Game Modes'/Starfield.XmlScript/CPL/CPLImportParser.g
java org.antlr.Tool ../'Game Modes'/Starfield.XmlScript/CPL/*.g
