#include <iostream>
#include <fstream>

#include "hlslcc.h"

static std::vector<char> ReadBinaryFile(const char* fileName)
{
	std::ifstream ifs(fileName, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = ifs.tellg();

	std::vector<char> result(pos);

	ifs.seekg(0, std::ios::beg);
	ifs.read(&result[0], pos);

	return result;
}

int main(int argc, const char* argv[])
{
	auto shaderBytes = ReadBinaryFile(argv[1]);

	GLLang lang = (GLLang) std::stoi(argv[2]);

	GlExtensions glExtensions;

	GLSLCrossDependencyData dependencies;

	HLSLccSamplerPrecisionInfo samplerPrecisions;
	HLSLccReflection reflectionCallbacks;

	GLSLShader result;

	auto success = TranslateHLSLFromMem(
		shaderBytes.data(),
		0,
		lang,
		&glExtensions,
		&dependencies,
		samplerPrecisions,
		reflectionCallbacks,
		&result);

	std::ofstream out(argv[3], std::ios::out);
	out << result.sourceCode;
	out.close();

	return 0;
}

