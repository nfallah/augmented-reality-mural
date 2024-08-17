#include <string>
#include <cstring>
#include "leveldb/db.h"

extern "C" {
    leveldb::DB* db=nullptr;
    bool db_open(const char *data_directory){
        std::string data_directory_str(data_directory);
        leveldb::Options options;
        options.create_if_missing=true;
        leveldb::Status status=leveldb::DB::Open(options, data_directory_str, &db);
        return status.ok();
    }
    bool db_close(){
        if (db!=nullptr){
            delete db;
            db=nullptr;
            return true;
        }
        return false;
    }
    const char *db_get(const char *key){
        std::string key_str(key);
        std::string value;
        leveldb::Status status=db->Get(leveldb::ReadOptions(), key_str, &value);
        if(!status.ok()){
            return nullptr;
        }
        char *result=new char[value.size()+1];
        std::strcpy(result, value.c_str());
        return result;
    }
    void db_free(const char *result){
        delete[] result;
    }
    bool db_put(const char *key, const char *value){
        std::string key_str(key);
        std::string value_str(value);
        leveldb::Status status=db->Put(leveldb::WriteOptions(), key_str, value_str);
        return status.ok();
    }
    bool db_delete(const char *key){
        std::string key_str(key);
        leveldb::Status status=db->Delete(leveldb::WriteOptions(), key_str);
        return status.ok();      
    }
}