// Used for memory-mapped functionality
#include <windows.h>
#include "sharedmemory.h"

// Used for this example
#include <stdio.h>
#include <conio.h>
#include <ctime>
#include <Sysinfoapi.h>
#include <fstream>
#include <string>
#include <thread>
// Name of the pCars memory mapped file
#define MAP_OBJECT_NAME "$pcars2$"

int main()
{
	Sleep(5000);
	// Init File Writer
	std::string accelx;
	std::string accely;
	std::fstream file;
	
	
	long long timestamp, startTimeMs;
	std::string timestampString;
	const char* timestampStringPtr = timestampString.c_str();
	const char* accelxStringPtr = accelx.c_str();
	const char* accelyStringPtr = accely.c_str();
	char* filename = new char[18]; //mm_ss_fff_raw.csv
	SYSTEMTIME* timePtr = new SYSTEMTIME();

	GetSystemTime(timePtr);
	startTimeMs = (long long)(timePtr->wMinute) * 1000 * 60 +
		(long long)timePtr->wSecond * 1000 +
		timePtr->wMilliseconds;

	sprintf(filename, "%d_%d_%d_raw.csv", timePtr->wMinute, timePtr->wSecond, timePtr->wMilliseconds);
	file.open(filename, std::ios::out | std::ios::trunc);

	// Open the memory-mapped file
	HANDLE fileHandle = OpenFileMapping( PAGE_READONLY, FALSE, MAP_OBJECT_NAME );
	if (fileHandle == NULL)
	{
		printf( "Could not open file mapping object (%d).\n", GetLastError() );
		return 1;
	}

	// Get the data structure
	const SharedMemory* sharedData = (SharedMemory*)MapViewOfFile( fileHandle, PAGE_READONLY, 0, 0, sizeof(SharedMemory) );
	SharedMemory* localCopy = new SharedMemory;
	if (sharedData == NULL)
	{
		printf( "Could not map view of file (%d).\n", GetLastError() );

		CloseHandle( fileHandle );
		return 1;
	}

	// Ensure we're sync'd to the correct data version
	/*if ( sharedData->mVersion != SHARED_MEMORY_VERSION )
	{
		printf( "Data version mismatch\n");
		return 1;
	}*/


	//------------------------------------------------------------------------------
	// TEST DISPLAY CODE
	//------------------------------------------------------------------------------
	unsigned int updateIndex(0);
	unsigned int indexChange(0);
	printf( "ESC TO EXIT\n\n" );
	while (true)
	{
		if ( sharedData->mSequenceNumber % 2 )
		{
			// Odd sequence number indicates, that write into the shared memory is just happening
			continue;
		}

		indexChange = sharedData->mSequenceNumber - updateIndex;
		updateIndex = sharedData->mSequenceNumber;

		//Copy the whole structure before processing it, otherwise the risk of the game writing into it during processing is too high.
		memcpy(localCopy,sharedData,sizeof(SharedMemory));


		if (localCopy->mSequenceNumber != updateIndex )
		{
			// More writes had happened during the read. Should be rare, but can happen.
			continue;
		}
		GetSystemTime(timePtr);

		timestamp = (long long)(timePtr->wMinute) * 1000 * 60 + 
			(long long) timePtr->wSecond * 1000 +
			timePtr->wMilliseconds - startTimeMs;

		printf( "Sequence number increase %d, current index %d, previous index %d\n", indexChange, localCopy->mSequenceNumber, updateIndex );

		const bool isValidParticipantIndex = localCopy->mViewedParticipantIndex != -1 && localCopy->mViewedParticipantIndex < localCopy->mNumParticipants && localCopy->mViewedParticipantIndex < STORED_PARTICIPANTS_MAX;
		if ( isValidParticipantIndex )
		{
			const ParticipantInfo& viewedParticipantInfo = localCopy->mParticipantInfo[sharedData->mViewedParticipantIndex];
			printf( "mParticipantName: (%s)\n", viewedParticipantInfo.mName );
			printf( "lap Distance = %f \n", viewedParticipantInfo.mCurrentLapDistance );
		}
		
		
		printf( "mGameState: (%d)\n", localCopy->mGameState );
		printf( "mSessionState: (%d)\n", localCopy->mSessionState );
		printf( "mOdometerKM: (%0.2f)\n", localCopy->mOdometerKM );

		accelx = "," + std::to_string(localCopy->mLocalAcceleration[0]);
		accely = "," + std::to_string(localCopy->mLocalAcceleration[2]) + '\n';
		timestampString = std::to_string(timestamp);
		file.write(timestampStringPtr, timestampString.size());
		file.write(accelxStringPtr, accelx.size());
		file.write(accelyStringPtr, accely.size());
		system("cls");

		if ( _kbhit() && _getch() == 27 ) // check for escape
		{
			break;
		}
	}
	//------------------------------------------------------------------------------

	// Cleanup
	UnmapViewOfFile( sharedData );
	CloseHandle( fileHandle );
	file.close();
	delete localCopy;

	return 0;
}
